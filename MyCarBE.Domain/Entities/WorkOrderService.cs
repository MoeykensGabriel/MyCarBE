using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

public class WorkOrderService : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    // Nullable: si es un servicio ad-hoc (puntual, no del catálogo), queda null.
    // En ese caso los datos viven solo en los snapshots de abajo.
    public Guid? CatalogServiceId { get; set; }
    public CatalogService? CatalogService { get; set; }

    // Snapshots tomados al momento de agregar — no se propagan cambios posteriores.
    // Para servicios ad-hoc son la única fuente de verdad.
    public string  NameSnapshot                     { get; set; } = string.Empty;
    public string  DescriptionSnapshot              { get; set; } = string.Empty;
    public decimal PriceSnapshot                    { get; set; }
    public int     EstimatedDurationMinutesSnapshot { get; set; }

    public int Quantity { get; set; } = 1;

    // ── Asignación al mecánico ────────────────────────────────────────────────
    public Guid? AssignedMechanicId { get; set; }
    public Mechanic? AssignedMechanic { get; set; }

    public WorkOrderServiceAssignmentStatus AssignmentStatus { get; set; }
        = WorkOrderServiceAssignmentStatus.Unassigned;

    public DateTime? AcceptedAt    { get; set; }
    public DateTime? CompletedAt   { get; set; }
    public string?   MechanicNotes { get; set; } // obligatorio al completar
    public string?   MechanicFindings { get; set; } // opcional, recomendaciones detectadas

    // Navegación
    public ICollection<WorkOrderPhoto> Photos { get; set; } = new List<WorkOrderPhoto>();

    // -------------------------------------------------------------------------
    // Comportamiento de asignación
    // -------------------------------------------------------------------------

    /// <summary>
    /// El admin asigna o reasigna un mecánico a este servicio.
    /// Si el servicio ya estaba Completed, se rechaza.
    /// Si venía Accepted o Pending, se reinicia a Pending y resetea AcceptedAt.
    /// </summary>
    public void AssignMechanic(Guid mechanicId)
    {
        if (AssignmentStatus == WorkOrderServiceAssignmentStatus.Completed)
            throw new InvalidOperationException("No se puede reasignar un servicio que ya fue finalizado.");

        AssignedMechanicId = mechanicId;
        AssignmentStatus   = WorkOrderServiceAssignmentStatus.Pending;
        AcceptedAt         = null;
    }

    /// <summary>
    /// El admin desasigna al mecánico. Solo válido en Pending o Accepted.
    /// </summary>
    public void Unassign()
    {
        if (AssignmentStatus == WorkOrderServiceAssignmentStatus.Completed)
            throw new InvalidOperationException("No se puede desasignar un servicio finalizado.");

        AssignedMechanicId = null;
        AssignmentStatus   = WorkOrderServiceAssignmentStatus.Unassigned;
        AcceptedAt         = null;
    }

    /// <summary>
    /// El mecánico asignado acepta el trabajo. Solo válido desde Pending.
    /// </summary>
    public void AcceptByMechanic(Guid mechanicId)
    {
        if (AssignedMechanicId != mechanicId)
            throw new InvalidOperationException("Este servicio no está asignado a este mecánico.");

        if (AssignmentStatus != WorkOrderServiceAssignmentStatus.Pending)
            throw new InvalidOperationException(
                $"Solo se puede aceptar un servicio en estado Pending. Estado actual: {AssignmentStatus}.");

        AssignmentStatus = WorkOrderServiceAssignmentStatus.Accepted;
        AcceptedAt       = DateTime.UtcNow;
    }

    /// <summary>
    /// El mecánico finaliza el servicio con notas obligatorias.
    /// Solo válido desde Accepted.
    /// </summary>
    public void CompleteByMechanic(Guid mechanicId, string notes, string? findings)
    {
        if (AssignedMechanicId != mechanicId)
            throw new InvalidOperationException("Este servicio no está asignado a este mecánico.");

        if (AssignmentStatus != WorkOrderServiceAssignmentStatus.Accepted)
            throw new InvalidOperationException(
                $"Solo se puede finalizar un servicio en estado Accepted. Estado actual: {AssignmentStatus}.");

        if (string.IsNullOrWhiteSpace(notes))
            throw new InvalidOperationException("Las notas son obligatorias al finalizar un servicio.");

        AssignmentStatus = WorkOrderServiceAssignmentStatus.Completed;
        CompletedAt      = DateTime.UtcNow;
        MechanicNotes    = notes.Trim();
        MechanicFindings = string.IsNullOrWhiteSpace(findings) ? null : findings.Trim();
    }
}
