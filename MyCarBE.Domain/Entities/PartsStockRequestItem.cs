using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Un repuesto puntual dentro de un pedido al depósito. Snapshot de los datos del
/// WorkOrderPart al momento de la aprobación — si el WorkOrderPart cambia después,
/// este item no se actualiza (consistencia con lo que se le envió a GestionPGB).
/// </summary>
public class PartsStockRequestItem : BaseEntity
{
    public Guid PartsStockRequestId { get; set; }
    public PartsStockRequest PartsStockRequest { get; set; } = null!;

    /// <summary>Traceability: de qué WorkOrderPart salió este item.</summary>
    public Guid WorkOrderPartId { get; set; }

    /// <summary>Código GestionPGB del repuesto (snapshot, requerido en items del pedido).</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>Nombre legible (snapshot para evitar joins en la pantalla de seguimiento).</summary>
    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public StockRequestItemStatus Status { get; set; } = StockRequestItemStatus.PendingReview;

    /// <summary>Nota opcional del depósito (ej: "Proveedor estima 3 días", "Sin stock confirmado").</summary>
    public string? Notes { get; set; }

    /// <summary>
    /// El depósito actualiza el estado del item. Se acepta cualquier transición porque
    /// GestionPGB es la fuente de verdad: si dice "fue entregado y vuelve a Missing"
    /// (porque se perdió en el envío), confiamos en el depósito.
    /// </summary>
    public void UpdateStatus(StockRequestItemStatus newStatus, string? notes)
    {
        Status = newStatus;
        Notes  = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
