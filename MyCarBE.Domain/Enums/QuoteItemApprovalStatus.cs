namespace MyCarBE.Domain.Enums;

/// <summary>
/// Estado de aprobación item-por-item del presupuesto.
/// Aplica tanto a WorkOrderPart como a WorkOrderService cuando se hace cotización item-by-item.
/// </summary>
public enum QuoteItemApprovalStatus
{
    Pending  = 0, // Default: aún no se envió a aprobación, o el cliente no decidió
    Approved = 1, // El cliente lo aprobó
    Rejected = 2  // El cliente lo rechazó
}
