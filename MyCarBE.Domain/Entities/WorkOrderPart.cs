using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Repuesto cargado en una orden de trabajo. A diferencia de WorkOrderService (mano de obra),
/// los repuestos viven solo en la orden — no hay catálogo de repuestos en MyCarApp. El código
/// de producto (ProductCode) referencia el barcode del Sistema de Stock externo (GestionPGB).
///
/// Si ProductCode es null, el repuesto es "custom" (compra suelta del taller, no pasa por el
/// depósito) y no se envía al Sistema de Stock al aprobar el presupuesto.
/// </summary>
public class WorkOrderPart : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    /// <summary>
    /// Código de barras / código de proveedor en GestionPGB. Si es null el repuesto es custom
    /// y no se envía al depósito.
    /// </summary>
    public string? ProductCode { get; set; }

    /// <summary>Descripción libre del repuesto (lo que ve el cliente en el presupuesto).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Precio de venta unitario del repuesto (el que ve y paga el cliente). Lo fija la oficina
    /// consultándolo con el admin por fuera del sistema. Editable hasta enviar el presupuesto.
    /// </summary>
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; } = 1;

    /// <summary>Subtotal del repuesto (precio × cantidad). Es lo que va al PDF y al total.</summary>
    public decimal Subtotal => UnitPrice * Quantity;

    public WorkOrderPartTier Tier { get; set; } = WorkOrderPartTier.Generic;

    public QuoteItemApprovalStatus ApprovalStatus { get; set; } = QuoteItemApprovalStatus.Pending;

    /// <summary>
    /// Cuando el presupuesto se envía al cliente (S4-05), se setea FrozenAt y los campos del
    /// repuesto pasan a ser inmutables. Si vuelve a Diagnosing por alguna razón, se limpia.
    /// </summary>
    public DateTime? FrozenAt { get; set; }
}
