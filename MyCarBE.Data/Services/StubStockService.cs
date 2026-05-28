using Microsoft.Extensions.Logging;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Services;

/// <summary>
/// Implementación stub mientras GestionPGB no esté integrado. Acepta cualquier pedido,
/// devuelve un ExternalReference simulado, y deja todos los items en PendingReview.
/// Los avances reales tienen que llegar por el webhook (lo cual permite testear el
/// flujo de seguimiento end-to-end con curl/Postman sin necesidad del sistema externo).
/// </summary>
public class StubStockService : IStockService
{
    private readonly ILogger<StubStockService> _logger;

    public StubStockService(ILogger<StubStockService> logger)
    {
        _logger = logger;
    }

    public Task<StockSubmissionResult> SubmitRequestAsync(
        PartsStockRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[StubStockService] Pedido simulado a GestionPGB — WorkOrderId={WorkOrderId} Patente={Plate} Items={ItemCount}",
            request.WorkOrderId, request.LicensePlateSnapshot, request.Items.Count);

        var itemStatuses = request.Items
            .Select(i => new StockItemInitialStatus(i.Id, StockRequestItemStatus.PendingReview, null))
            .ToList();

        var result = new StockSubmissionResult(
            Success:           true,
            ExternalReference: $"STUB-{Guid.NewGuid():N}".Substring(0, 16),
            ItemStatuses:      itemStatuses);

        return Task.FromResult(result);
    }
}
