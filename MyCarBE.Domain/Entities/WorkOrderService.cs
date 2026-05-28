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

    // ── Cotización item-por-item ───────────────────────────────────────────────

    /// <summary>
    /// Estado de aprobación item-por-item. Mientras no se envía el presupuesto queda Pending.
    /// El cliente lo mueve a Approved/Rejected al aprobar item-by-item (S4-06).
    /// </summary>
    public QuoteItemApprovalStatus ApprovalStatus { get; set; }
        = QuoteItemApprovalStatus.Pending;

    /// <summary>
    /// Si tiene valor, este servicio forma parte de un grupo de alternativas mutuamente
    /// excluyentes (mano de obra alternativa: ej "Cambio completo" vs "Reparación parcial").
    /// El cliente debe elegir exactamente uno por grupo al aprobar.
    /// </summary>
    public Guid? AlternativeGroupId { get; set; }

    /// <summary>
    /// Cuando el presupuesto se envía al cliente (S4-05), se setea FrozenAt y los datos del
    /// servicio pasan a ser inmutables hasta que la orden vuelva a Diagnosing.
    /// </summary>
    public DateTime? FrozenAt { get; set; }

    /// <summary>
    /// Área del taller a la que pertenece este servicio (Motor, Frenos, Tren delantero, etc.).
    /// Se setea automáticamente al convertir una propuesta de inspección (heredando el área
    /// del InspectionReport). En servicios ad-hoc puede ser null o setearlo manualmente.
    /// El calendario agrupa por esta columna.
    /// </summary>
    public Guid? AreaId { get; set; }
    public Area? Area { get; set; }

    // ── Scheduling (calendario por área) ─────────────────────────────────────

    /// <summary>
    /// Fecha de inicio del trabajo. Setea el admin al agendar.
    /// El calendario del taller usa [ScheduledStart, ScheduledEnd] para mostrar qué
    /// patente ocupa qué mecánico durante qué días.
    /// </summary>
    public DateTime? ScheduledStart { get; set; }

    /// <summary>
    /// Fecha de fin estimada. Por defecto se calcula desde ScheduledStart + EstimatedDays,
    /// pero el admin puede ajustarla manualmente si el trabajo se extiende.
    /// </summary>
    public DateTime? ScheduledEnd { get; set; }

    /// <summary>
    /// Cuántos días estima el mecánico que va a durar este trabajo (lo sugiere en su
    /// inspección y se copia al convertir la propuesta). Sirve como default para
    /// calcular ScheduledEnd cuando el admin agenda el inicio.
    /// </summary>
    public int? EstimatedDays { get; set; }

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
    /// El mecánico se auto-asigna un servicio del pool. Solo válido si el servicio
    /// no tiene mecánico asignado, está en estado Unassigned, y fue aprobado por el cliente.
    /// El estado pasa a Pending — el mecánico todavía tiene que aceptar formalmente
    /// (AcceptByMechanic) antes de empezar a trabajar; esto le permite liberar el
    /// servicio si se equivoca.
    ///
    /// NOTA: La race condition con dos mecánicos clickeando "Tomar" al mismo tiempo
    /// se resuelve a nivel persistencia con el concurrency token `xmin` de Postgres
    /// (ver WorkOrderServiceConfiguration). El segundo SaveChangesAsync tira
    /// DbUpdateConcurrencyException que el handler traduce a 409 Conflict.
    /// </summary>
    public void ClaimByMechanic(Guid mechanicId)
    {
        if (AssignmentStatus != WorkOrderServiceAssignmentStatus.Unassigned)
            throw new InvalidOperationException(
                $"Este servicio ya fue tomado (estado actual: {AssignmentStatus}).");

        if (AssignedMechanicId != null)
            throw new InvalidOperationException("Este servicio ya tiene mecánico asignado.");

        if (ApprovalStatus != QuoteItemApprovalStatus.Approved)
            throw new InvalidOperationException(
                "Solo se pueden tomar servicios aprobados por el cliente.");

        AssignedMechanicId = mechanicId;
        AssignmentStatus   = WorkOrderServiceAssignmentStatus.Pending;
        AcceptedAt         = null;
    }

    /// <summary>
    /// El mecánico libera un servicio que se había tomado pero todavía no aceptó.
    /// Solo válido si el servicio está en Pending y le pertenece al llamador.
    /// Vuelve al pool (Unassigned + AssignedMechanicId = null).
    /// </summary>
    public void ReleaseByMechanic(Guid mechanicId)
    {
        if (AssignedMechanicId != mechanicId)
            throw new InvalidOperationException("Este servicio no está asignado a este mecánico.");

        if (AssignmentStatus != WorkOrderServiceAssignmentStatus.Pending)
            throw new InvalidOperationException(
                $"Solo se puede liberar un servicio en estado Pending. Estado actual: {AssignmentStatus}.");

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
