using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Cliente del Sistema de Stock externo (GestionPGB). Se inyecta como dependencia para que
/// el resto del código no sepa si está hablando con un mock, HTTP, gRPC o lo que sea.
///
/// Implementación inicial: StubStockService — todo queda PendingReview hasta que el depósito
/// real esté listo y haga callbacks vía webhook.
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Envía un pedido al depósito y devuelve la respuesta inicial. El depósito puede:
    ///   - Aceptar y dejarlo en revisión → todos los items quedan PendingReview
    ///   - Responder inmediatamente con qué tiene en stock y qué falta
    /// El llamador aplica los estados al PartsStockRequest y persiste.
    /// </summary>
    Task<StockSubmissionResult> SubmitRequestAsync(
        PartsStockRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>Respuesta de GestionPGB al recibir un pedido.</summary>
public record StockSubmissionResult(
    bool Success,
    string? ExternalReference,
    IReadOnlyList<StockItemInitialStatus> ItemStatuses,
    string? Error = null);

/// <summary>Estado inicial que devuelve el depósito por cada item del pedido.</summary>
public record StockItemInitialStatus(
    Guid ItemId,
    StockRequestItemStatus Status,
    string? Notes);
