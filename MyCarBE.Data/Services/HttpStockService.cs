using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Services;

/// <summary>
/// Cliente HTTP del Sistema de Stock externo (GestionPGB).
/// Configurado vía appsettings:StockSystem
///   BaseUrl         = http://localhost:5000
///   ApiKey          = clave para enviar pedidos al depósito (header X-Api-Key)
///   CallbackBaseUrl = nuestra URL pública para que el depósito haga callbacks
///
/// Habla con `POST /api/workshop-orders` del depósito y mapea su respuesta (con enums
/// como strings) a nuestro modelo interno.
/// </summary>
public class HttpStockService : IStockService
{
    private readonly HttpClient                _httpClient;
    private readonly IConfiguration            _configuration;
    private readonly ILogger<HttpStockService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters           = { new JsonStringEnumConverter() },
    };

    public HttpStockService(
        HttpClient                httpClient,
        IConfiguration            configuration,
        ILogger<HttpStockService> logger)
    {
        _httpClient    = httpClient;
        _configuration = configuration;
        _logger        = logger;
    }

    public async Task<StockSubmissionResult> SubmitRequestAsync(
        PartsStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey          = _configuration["StockSystem:ApiKey"];
        var callbackBaseUrl = _configuration["StockSystem:CallbackBaseUrl"]?.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("StockSystem:ApiKey no está configurado. No se puede enviar pedido a GestionPGB.");
            return new StockSubmissionResult(false, null, Array.Empty<StockItemInitialStatus>(), "API key no configurada.");
        }

        // El depósito va a hacer callback a esta URL cuando confirme la entrega.
        // Si no tenemos callbackBaseUrl configurada, mandamos null y el callback no llega
        // (la oficina puede usar el botón de Sync manual mientras tanto).
        var callbackUrl = string.IsNullOrWhiteSpace(callbackBaseUrl)
            ? null
            : $"{callbackBaseUrl}/api/stock-requests/callback";

        var payload = new
        {
            licensePlate = request.LicensePlateSnapshot,
            callbackUrl,
            // Condición de venta (fase B): CC / OC + número / Contado + seña.
            // GestionPGB tiene el mismo enum con los mismos nombres — viaja como string.
            condition     = request.SaleConditionSnapshot?.ToString(),
            ocNumber      = request.OcNumberSnapshot,
            depositAmount = request.DepositAmountSnapshot,
            items        = request.Items.Select(i => new
            {
                productCode = i.ProductCode,
                quantity    = i.Quantity,
            }).ToList(),
        };

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/workshop-orders")
            {
                Content = JsonContent.Create(payload, options: JsonOptions),
            };
            httpRequest.Headers.Add("X-Api-Key", apiKey);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "GestionPGB devolvió {StatusCode} para WorkOrder {WorkOrderId}: {Body}",
                    response.StatusCode, request.WorkOrderId, error);

                return new StockSubmissionResult(
                    Success:           false,
                    ExternalReference: null,
                    ItemStatuses:      Array.Empty<StockItemInitialStatus>(),
                    Error:             $"GestionPGB respondió {(int)response.StatusCode}: {error}");
            }

            var workshopOrder = await response.Content.ReadFromJsonAsync<WorkshopOrderResponse>(
                JsonOptions, cancellationToken);

            if (workshopOrder is null)
            {
                return new StockSubmissionResult(false, null, Array.Empty<StockItemInitialStatus>(),
                    "GestionPGB devolvió un body vacío.");
            }

            // Mapear cada item devuelto al ID interno nuestro vía matching por ProductCode.
            // (GestionPGB no conoce nuestros Guids; cruzamos por código de barras.)
            var initialStatuses = new List<StockItemInitialStatus>();
            foreach (var ourItem in request.Items)
            {
                var theirItem = workshopOrder.Items
                    .FirstOrDefault(i => string.Equals(i.ProductCode, ourItem.ProductCode, StringComparison.Ordinal));

                if (theirItem is null)
                {
                    _logger.LogWarning(
                        "GestionPGB no devolvió item para ProductCode {Code} (WorkOrder {WorkOrderId}). Queda como Missing.",
                        ourItem.ProductCode, request.WorkOrderId);
                    initialStatuses.Add(new StockItemInitialStatus(
                        ourItem.Id, StockRequestItemStatus.Missing,
                        "El depósito no devolvió respuesta para este código."));
                    continue;
                }

                var (status, notes) = MapItemStatus(theirItem);
                initialStatuses.Add(new StockItemInitialStatus(ourItem.Id, status, notes));
            }

            return new StockSubmissionResult(
                Success:           true,
                ExternalReference: workshopOrder.Id.ToString(),
                ItemStatuses:      initialStatuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fallo conectando con GestionPGB para WorkOrder {WorkOrderId}. ¿Está corriendo en {BaseUrl}?",
                request.WorkOrderId, _httpClient.BaseAddress);

            return new StockSubmissionResult(false, null, Array.Empty<StockItemInitialStatus>(), ex.Message);
        }
    }

    /// <summary>
    /// Mapea el estado item-level de GestionPGB al nuestro.
    /// Available     → Available
    /// Shortage      → Missing (notes: "Sin stock")
    /// NotFound      → Missing (notes: "Código no existe en el depósito")
    /// PurchasedInTransit → InTransit
    /// Delivered     → Delivered
    /// </summary>
    private static (StockRequestItemStatus status, string? notes) MapItemStatus(WorkshopOrderItemResponse item)
    {
        return item.Status switch
        {
            "Available"          => (StockRequestItemStatus.Available, null),
            "Shortage"           => (StockRequestItemStatus.Missing,   $"Falta {item.ShortageQuantity} unidad(es) — el depósito debe comprar."),
            "NotFound"           => (StockRequestItemStatus.Missing,   "El código no existe en el depósito. Verificar con la oficina."),
            "PurchasedInTransit" => (StockRequestItemStatus.InTransit, null),
            "Delivered"          => (StockRequestItemStatus.Delivered, null),
            _                    => (StockRequestItemStatus.PendingReview, $"Estado desconocido: {item.Status}"),
        };
    }

    // ── DTOs locales para deserializar la respuesta de GestionPGB ────────────

    private sealed record WorkshopOrderResponse(
        Guid                            Id,
        string                          LicensePlate,
        string                          Status,
        string?                         CallbackUrl,
        DateTime                        CreatedAt,
        DateTime                        UpdatedAt,
        List<WorkshopOrderItemResponse> Items);

    private sealed record WorkshopOrderItemResponse(
        Guid    Id,
        string  ProductCode,
        string? ProductName,
        string? ProviderName,
        int     RequestedQuantity,
        int     ReservedQuantity,
        int     ShortageQuantity,
        string  Status,
        DateTime UpdatedAt);
}
