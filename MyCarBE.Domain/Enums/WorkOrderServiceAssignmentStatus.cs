namespace MyCarBE.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida de un WorkOrderService respecto al mecánico asignado.
/// Independiente del WorkOrderStatus (que aplica a la orden completa).
/// </summary>
public enum WorkOrderServiceAssignmentStatus
{
    Unassigned = 0, // Sin mecánico asignado
    Pending    = 1, // Asignado pero el mecánico todavía no aceptó
    Accepted   = 2, // El mecánico aceptó y está trabajando
    Completed  = 3  // El mecánico finalizó (con notas obligatorias)
}
