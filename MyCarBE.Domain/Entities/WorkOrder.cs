using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

public class WorkOrder : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    // Snapshots del dueño al momento de apertura — se congelan y nunca se modifican
    public Guid? CustomerIdAtEntry { get; set; }
    public Customer? CustomerAtEntry { get; set; }

    public Guid? FleetIdAtEntry { get; set; }
    public Fleet? FleetAtEntry { get; set; }

    public int MileageAtEntry { get; set; }
    public string? CustomerNote { get; set; }
    public string? TechnicianNote { get; set; }

    /// <summary>
    /// Motivo por el que el cliente trae el vehículo (texto libre).
    /// Nullable a nivel DB para no romper órdenes históricas; el validador del CreateWorkOrderCommand
    /// lo exige a partir de S3-14.
    /// </summary>
    public string? ServiceReason { get; set; }

    // Empleado que trajo el vehículo (solo para flotas, opcional)
    public string? ContactPersonName { get; set; }
    public string? ContactPersonPhone { get; set; }

    // Denormalizado para queries rápidas — la fuente de verdad es StatusChanges
    public WorkOrderStatus CurrentStatus { get; set; }

    public decimal TotalAmount { get; set; }

    // Usuario que creó la orden (Admin o Receptionist). Nullable porque órdenes previas a este campo no lo tienen.
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Fecha en que vence la posibilidad de aprobar el presupuesto. Se setea al enviarlo
    /// (S4-05) en now + 30 días. Después de esa fecha, el TTL cleanup (S4-07) transiciona
    /// la orden a Cancelled si sigue en AwaitingApproval.
    /// </summary>
    public DateTime? QuoteExpiresAt { get; set; }

    // Navegación
    public ICollection<WorkOrderService> Services { get; set; } = new List<WorkOrderService>();
    public ICollection<WorkOrderPart> Parts { get; set; } = new List<WorkOrderPart>();
    public ICollection<WorkOrderPhoto> Photos { get; set; } = new List<WorkOrderPhoto>();
    public ICollection<WorkOrderStatusChange> StatusChanges { get; set; } = new List<WorkOrderStatusChange>();
    public ICollection<InspectionReport> InspectionReports { get; set; } = new List<InspectionReport>();

    // -------------------------------------------------------------------------
    // Máquina de estados
    // -------------------------------------------------------------------------

    private static readonly Dictionary<WorkOrderStatus, WorkOrderStatus[]> ValidTransitions = new()
    {
        // Estado inicial de nuevas órdenes a partir de S3-06. Pasa a Diagnosing cuando el admin
        // cierra la inspección colectiva (todas las áreas reportaron o fueron marcadas "sin hallazgos").
        { WorkOrderStatus.UnderInspection,  new[] { WorkOrderStatus.Diagnosing, WorkOrderStatus.Cancelled } },
        // Received queda como estado legacy de órdenes pre-S3-06. Permitimos que pasen a UnderInspection
        // o directo a Diagnosing/Cancelled para no trabar nada.
        { WorkOrderStatus.Received,         new[] { WorkOrderStatus.UnderInspection, WorkOrderStatus.Diagnosing, WorkOrderStatus.Cancelled } },
        { WorkOrderStatus.Diagnosing,        new[] { WorkOrderStatus.AwaitingApproval, WorkOrderStatus.Cancelled } },
        // AwaitingApproval ahora pasa a Approved (el cliente aprobó pero el auto puede no haber llegado).
        // Mantenemos también el atajo directo a InProgress por si el admin quiere saltearlo.
        { WorkOrderStatus.AwaitingApproval, new[] { WorkOrderStatus.Approved, WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled } },
        // Aprobado: ya hay luz verde del cliente. Cuando llega el vehículo / arranca el trabajo, va a InProgress.
        { WorkOrderStatus.Approved,         new[] { WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled } },
        { WorkOrderStatus.InProgress,       new[] { WorkOrderStatus.Completed, WorkOrderStatus.Cancelled } },
        { WorkOrderStatus.Completed,        new[] { WorkOrderStatus.Delivered, WorkOrderStatus.Cancelled } },
        { WorkOrderStatus.Delivered,        Array.Empty<WorkOrderStatus>() },
        { WorkOrderStatus.Cancelled,        Array.Empty<WorkOrderStatus>() },
    };

    /// <summary>
    /// Cambia el estado de la orden validando que la transición sea permitida.
    /// La cancelación requiere nota obligatoria.
    /// Registra el evento en StatusChanges y actualiza CurrentStatus.
    /// </summary>
    public void ChangeStatus(WorkOrderStatus newStatus, Guid changedByUserId, string? note = null)
    {
        if (newStatus == WorkOrderStatus.Cancelled && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("A note is required when cancelling a work order.");

        if (!ValidTransitions.TryGetValue(CurrentStatus, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException(
                $"Invalid transition: cannot move from '{CurrentStatus}' to '{newStatus}'.");

        // Regla de negocio: solo se puede pasar a Completed si TODOS los servicios
        // activos fueron finalizados por sus mecánicos.
        if (newStatus == WorkOrderStatus.Completed)
        {
            var pending = Services
                .Where(s => !s.IsDeleted &&
                            s.AssignmentStatus != Enums.WorkOrderServiceAssignmentStatus.Completed)
                .ToList();

            if (pending.Count > 0)
                throw new InvalidOperationException(
                    "No se puede completar la orden: hay servicios pendientes de finalizar por mecánicos.");
        }

        // Regla de negocio: para entregar el vehículo al cliente, debe haber al menos UNA foto
        // After del vehículo entero (no de un servicio puntual). Esto fuerza la documentación
        // del estado de devolución que el cliente espera ver en el detalle de la orden.
        if (newStatus == WorkOrderStatus.Delivered)
        {
            var hasAfterPhoto = Photos.Any(p =>
                !p.IsDeleted &&
                p.PhotoType == Enums.PhotoType.After &&
                p.WorkOrderServiceId == null &&
                p.InspectionReportId == null);

            if (!hasAfterPhoto)
                throw new InvalidOperationException(
                    "Antes de entregar el vehículo, cargá al menos una foto del estado final (After) del auto.");
        }

        StatusChanges.Add(new WorkOrderStatusChange
        {
            WorkOrderId = Id,
            FromStatus = CurrentStatus,
            ToStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId,
            Note = note
        });

        CurrentStatus = newStatus;
    }

    /// <summary>
    /// Inicializa la orden con estado UnderInspection y registra el primer evento (FromStatus = null).
    /// Llamar una sola vez al crear la WorkOrder.
    /// (A partir de S3-06 las nuevas órdenes arrancan directamente en UnderInspection — el estado
    /// 'Received' queda como legacy para órdenes históricas.)
    /// </summary>
    public void Initialize(Guid createdByUserId)
    {
        if (StatusChanges.Any())
            throw new InvalidOperationException("WorkOrder has already been initialized.");

        CurrentStatus = WorkOrderStatus.UnderInspection;

        StatusChanges.Add(new WorkOrderStatusChange
        {
            Id = Guid.NewGuid(),
            WorkOrderId = Id,
            FromStatus = null,
            ToStatus = WorkOrderStatus.UnderInspection,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = createdByUserId
        });
    }

    /// <summary>
    /// Recalcula TotalAmount sumando items activos excluyendo los Rejected por el cliente.
    /// Mientras todo está Pending (antes de aprobar) o todo Approved (post aprobación), suma
    /// lo que está vivo. Items Rejected quedan visibles en el detalle pero no cuentan al total.
    /// </summary>
    public void RecalculateTotalAmount()
    {
        var servicesTotal = Services
            .Where(s => !s.IsDeleted && s.ApprovalStatus != Enums.QuoteItemApprovalStatus.Rejected)
            .Sum(s => s.PriceSnapshot * s.Quantity);

        // Los repuestos suman al total con el PRECIO DE CLIENTE (markup) — que es lo que ve y
        // aprueba el cliente en el PDF. Si no hay precio de cliente, cae al costo interno.
        var partsTotal = Parts
            .Where(p => !p.IsDeleted && p.ApprovalStatus != Enums.QuoteItemApprovalStatus.Rejected)
            .Sum(p => p.CustomerSubtotal);

        TotalAmount = servicesTotal + partsTotal;
    }

    /// <summary>
    /// Aplica la decisión del cliente al presupuesto: marca items como Approved/Rejected,
    /// valida la coherencia de los grupos de alternativas, y recalcula el total.
    ///
    /// Reglas:
    ///   - El estado actual debe ser AwaitingApproval.
    ///   - Todo ID en las listas tiene que existir como item activo de esta WO.
    ///   - Cada AlternativeGroupId presente entre los items debe tener EXACTAMENTE 1 aprobado.
    ///   - Tiene que haber al menos 1 item aprobado en total.
    ///   - Items NO incluidos en las listas se marcan Rejected (whitelist approach).
    ///
    /// NO cambia el estado de la WO — eso lo hace el caller con ChangeStatus(Approved).
    /// </summary>
    public void ApplyCustomerApproval(
        IEnumerable<Guid> approvedServiceIds,
        IEnumerable<Guid> approvedPartIds)
    {
        if (CurrentStatus != Enums.WorkOrderStatus.AwaitingApproval)
            throw new InvalidOperationException(
                $"La orden no está pendiente de aprobación. Estado actual: {CurrentStatus}.");

        var approvedSvcSet  = new HashSet<Guid>(approvedServiceIds);
        var approvedPartSet = new HashSet<Guid>(approvedPartIds);

        var activeServices = Services.Where(s => !s.IsDeleted).ToList();
        var activeParts    = Parts.Where(p => !p.IsDeleted).ToList();

        // 1) IDs deben pertenecer a items activos de esta WO
        var validSvcIds  = activeServices.Select(s => s.Id).ToHashSet();
        var validPartIds = activeParts.Select(p => p.Id).ToHashSet();

        if (approvedSvcSet.Any(id => !validSvcIds.Contains(id)))
            throw new InvalidOperationException("Hay servicios aprobados que no pertenecen a esta orden.");

        if (approvedPartSet.Any(id => !validPartIds.Contains(id)))
            throw new InvalidOperationException("Hay repuestos aprobados que no pertenecen a esta orden.");

        // 2) Cada grupo de alternativas: exactamente 1 elegido
        // Recolectamos por AlternativeGroupId todos los items (services + parts comparten namespace de grupo).
        var groupsToApproved = new Dictionary<Guid, int>();
        var groupsToTotal    = new Dictionary<Guid, int>();

        foreach (var s in activeServices)
        {
            if (!s.AlternativeGroupId.HasValue) continue;
            var gid = s.AlternativeGroupId.Value;
            groupsToTotal[gid]    = groupsToTotal.GetValueOrDefault(gid) + 1;
            if (approvedSvcSet.Contains(s.Id))
                groupsToApproved[gid] = groupsToApproved.GetValueOrDefault(gid) + 1;
        }
        foreach (var p in activeParts)
        {
            if (!p.AlternativeGroupId.HasValue) continue;
            var gid = p.AlternativeGroupId.Value;
            groupsToTotal[gid]    = groupsToTotal.GetValueOrDefault(gid) + 1;
            if (approvedPartSet.Contains(p.Id))
                groupsToApproved[gid] = groupsToApproved.GetValueOrDefault(gid) + 1;
        }

        foreach (var (gid, total) in groupsToTotal)
        {
            var picked = groupsToApproved.GetValueOrDefault(gid);
            if (picked != 1)
                throw new InvalidOperationException(
                    $"Cada grupo de alternativas requiere exactamente una elección. " +
                    $"Grupo {gid:N} tiene {total} opciones y {picked} aprobada(s).");
        }

        // 3) Mínimo 1 item aprobado
        if (approvedSvcSet.Count + approvedPartSet.Count == 0)
            throw new InvalidOperationException(
                "Tenés que aprobar al menos un item del presupuesto. Si no querés realizar el trabajo, contactá al taller.");

        // 4) Aplicar decisión: whitelist approach — todo lo no aprobado queda Rejected
        foreach (var s in activeServices)
            s.ApprovalStatus = approvedSvcSet.Contains(s.Id)
                ? Enums.QuoteItemApprovalStatus.Approved
                : Enums.QuoteItemApprovalStatus.Rejected;

        foreach (var p in activeParts)
            p.ApprovalStatus = approvedPartSet.Contains(p.Id)
                ? Enums.QuoteItemApprovalStatus.Approved
                : Enums.QuoteItemApprovalStatus.Rejected;

        RecalculateTotalAmount();
    }
}
