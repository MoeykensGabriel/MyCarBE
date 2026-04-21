using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Evento inmutable. No hereda de BaseEntity: no tiene soft delete ni UpdatedAt.
/// Solo se inserta — nunca se edita ni se borra.
/// Si hubo un error al cambiar estado, se compensa con otro evento.
/// </summary>
public class WorkOrderStatusChange
{
    public Guid Id { get; set; }
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    public WorkOrderStatus? FromStatus { get; set; }   // Null en el primer evento (Received)
    public WorkOrderStatus ToStatus { get; set; }

    public DateTime ChangedAt { get; set; }
    public Guid ChangedByUserId { get; set; }           // FK a ApplicationUser (solo Guid)
    public string? Note { get; set; }                   // Obligatoria si ToStatus == Cancelled
}
