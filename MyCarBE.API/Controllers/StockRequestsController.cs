using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCarBE.Application.Features.StockRequests.Commands.HandleStockCallback;
using MyCarBE.Application.Features.StockRequests.Commands.RetryStockSubmission;
using MyCarBE.Application.Features.StockRequests.Commands.UpdateStockRequestItem;
using MyCarBE.Application.Features.StockRequests.DTOs;
using MyCarBE.Application.Features.StockRequests.Queries.GetStockRequestById;
using MyCarBE.Application.Features.StockRequests.Queries.GetStockRequests;
using MyCarBE.Domain.Enums;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Endpoints de la integración con el Sistema de Stock externo (GestionPGB).
/// - Lectura: pantalla de seguimiento de la oficina (listado + detalle).
/// - Callback: GestionPGB nos avisa cuando confirma la entrega.
/// - Override manual: la oficina puede forzar un estado de item (útil mientras GestionPGB
///   no notifica estados intermedios via callback).
/// </summary>
[ApiController]
[Route("api/stock-requests")]
public class StockRequestsController : ControllerBase
{
    private readonly ISender                              _sender;
    private readonly IConfiguration                       _configuration;
    private readonly ILogger<StockRequestsController>     _logger;

    public StockRequestsController(
        ISender                          sender,
        IConfiguration                   configuration,
        ILogger<StockRequestsController> logger)
    {
        _sender        = sender;
        _configuration = configuration;
        _logger        = logger;
    }

    /// <summary>
    /// Listado de pedidos al depósito para la pantalla de seguimiento de la oficina.
    /// Filtros opcionales por estado agregado y patente.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockRequestDto>>> List(
        [FromQuery] StockRequestStatus? status,
        [FromQuery] string?             licensePlate,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockRequestsQuery(status, licensePlate), cancellationToken);
        return Ok(result);
    }

    /// <summary>Detalle de un pedido específico con sus items.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockRequestByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    public record UpdateItemBody(StockRequestItemStatus Status, string? Notes);

    /// <summary>
    /// Override manual de un item — usado por la oficina para forzar un estado mientras
    /// GestionPGB no notifica estados intermedios (solo manda callback en delivery final).
    /// </summary>
    [HttpPost("items/{itemId:guid}/status")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockRequestDto>> UpdateItemStatus(
        Guid                  itemId,
        [FromBody] UpdateItemBody body,
        CancellationToken     cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateStockRequestItemCommand(itemId, body.Status, body.Notes),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Reintenta el envío a GestionPGB para un pedido que quedó en PendingReview por fallo
    /// en la llamada original (p.ej. el depósito no estaba corriendo al momento de la aprobación).
    /// Solo tiene efecto si el pedido todavía no tiene ExternalReference.
    /// </summary>
    [HttpPost("{id:guid}/retry-submission")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockRequestDto>> RetrySubmission(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RetryStockSubmissionCommand(id), cancellationToken);
        return Ok(result);
    }

    public record StockCallbackBody(
        string                          LicensePlate,
        Guid                            WorkshopOrderId,
        string                          Status,
        DateTime                        DeliveredAt,
        IReadOnlyList<CallbackItemDto>  Items);

    public record CallbackItemDto(
        string ProductCode,
        int    RequestedQuantity,
        int    DeliveredQuantity,
        string Status);

    /// <summary>
    /// Endpoint que invoca GestionPGB cuando confirma la entrega de un pedido.
    /// Se autentica con el header X-Api-Key configurado en StockSystem:CallbackApiKey.
    /// </summary>
    [HttpPost("callback")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> HandleCallback(
        [FromBody] StockCallbackBody body,
        CancellationToken cancellationToken)
    {
        // Validación de API key inline (sin Authorize pipeline porque es machine-to-machine).
        var expectedKey = _configuration["StockSystem:CallbackApiKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            _logger.LogError("StockSystem:CallbackApiKey no está configurado — rechazando callback.");
            return Unauthorized();
        }

        if (!Request.Headers.TryGetValue("X-Api-Key", out var providedKey) ||
            !string.Equals(providedKey.ToString(), expectedKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Callback rechazado: X-Api-Key inválida o ausente desde {Ip}.",
                HttpContext.Connection.RemoteIpAddress);
            return Unauthorized();
        }

        await _sender.Send(
            new HandleStockCallbackCommand(
                body.LicensePlate,
                body.WorkshopOrderId,
                body.Status,
                body.DeliveredAt,
                body.Items
                    .Select(i => new CallbackItemPayload(
                        i.ProductCode, i.RequestedQuantity, i.DeliveredQuantity, i.Status))
                    .ToList()),
            cancellationToken);

        return NoContent();
    }
}
