using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.StockRequests.Services;

/// <summary>
/// Encapsula la creación del PartsStockRequest al aprobar una WorkOrder.
/// Se invoca desde los handlers de ApproveAsCustomer, ApproveWorkOrder y del shortcut
/// del admin en ChangeWorkOrderStatus para evitar duplicar lógica.
///
/// Es idempotente: si la WO ya tiene un pedido creado, no hace nada.
/// </summary>
public interface IStockRequestOrchestrator
{
    Task EnsureStockRequestForApprovedWorkOrderAsync(
        WorkOrder workOrder,
        CancellationToken cancellationToken = default);
}
