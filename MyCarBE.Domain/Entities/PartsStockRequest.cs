using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Pedido completo al Sistema de Stock (GestionPGB) por los repuestos de una WorkOrder
/// aprobada. Se crea automáticamente cuando la WO transiciona a Approved/InProgress
/// desde AwaitingApproval, agrupando todos los WorkOrderPart con ProductCode (los repuestos
/// custom sin código no se envían al depósito).
///
/// La patente se snapshot-ea — si el vehículo cambia de patente después, este pedido
/// conserva la patente original con la que se envió al depósito.
/// </summary>
public class PartsStockRequest : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    /// <summary>Patente del vehículo al momento del pedido (snapshot, lo que se le mandó a GestionPGB).</summary>
    public string LicensePlateSnapshot { get; set; } = string.Empty;

    /// <summary>
    /// ID que devolvió GestionPGB al recibir el pedido. Permite cruzar referencias entre
    /// sistemas si hay que investigar discrepancias. Nullable porque en la implementación stub
    /// no hay sistema externo.
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// Estado agregado denormalizado — derivado de los items via RecomputeStatus().
    /// Se actualiza cada vez que cambia un item para mantener queries rápidas.
    /// </summary>
    public StockRequestStatus Status { get; set; } = StockRequestStatus.PendingReview;

    public ICollection<PartsStockRequestItem> Items { get; set; } = new List<PartsStockRequestItem>();

    /// <summary>
    /// Recalcula el estado agregado a partir de los items. Reglas:
    ///   - Todos Delivered → Ready
    ///   - Algún Missing  → HasShortages (override: la oficina necesita ver bloqueos)
    ///   - Algún Available/InTransit/Delivered, ninguno Missing → InProgress
    ///   - Resto (todos PendingReview) → PendingReview
    /// </summary>
    public void RecomputeStatus()
    {
        var activeItems = Items.Where(i => !i.IsDeleted).ToList();

        if (activeItems.Count == 0)
        {
            Status = StockRequestStatus.PendingReview;
            return;
        }

        if (activeItems.All(i => i.Status == StockRequestItemStatus.Delivered))
        {
            Status = StockRequestStatus.Ready;
            return;
        }

        if (activeItems.Any(i => i.Status == StockRequestItemStatus.Missing))
        {
            Status = StockRequestStatus.HasShortages;
            return;
        }

        if (activeItems.Any(i =>
                i.Status == StockRequestItemStatus.Available ||
                i.Status == StockRequestItemStatus.InTransit ||
                i.Status == StockRequestItemStatus.Delivered))
        {
            Status = StockRequestStatus.InProgress;
            return;
        }

        Status = StockRequestStatus.PendingReview;
    }
}
