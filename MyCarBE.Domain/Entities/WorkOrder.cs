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

    // Empleado que trajo el vehículo (solo para flotas, opcional)
    public string? ContactPersonName { get; set; }
    public string? ContactPersonPhone { get; set; }

    // Denormalizado para queries rápidas — la fuente de verdad es StatusChanges
    public WorkOrderStatus CurrentStatus { get; set; }

    public decimal TotalAmount { get; set; }

    // Usuario que creó la orden (Admin o Receptionist). Nullable porque órdenes previas a este campo no lo tienen.
    public Guid? CreatedByUserId { get; set; }

    // Navegación
    public ICollection<WorkOrderService> Services { get; set; } = new List<WorkOrderService>();
    public ICollection<WorkOrderPhoto> Photos { get; set; } = new List<WorkOrderPhoto>();
    public ICollection<WorkOrderStatusChange> StatusChanges { get; set; } = new List<WorkOrderStatusChange>();

    // -------------------------------------------------------------------------
    // Máquina de estados
    // -------------------------------------------------------------------------

    private static readonly Dictionary<WorkOrderStatus, WorkOrderStatus[]> ValidTransitions = new()
    {
        { WorkOrderStatus.Received,         new[] { WorkOrderStatus.Diagnosing, WorkOrderStatus.Cancelled } },
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
    /// Inicializa la orden con estado Received y registra el primer evento (FromStatus = null).
    /// Llamar una sola vez al crear la WorkOrder.
    /// </summary>
    public void Initialize(Guid createdByUserId)
    {
        if (StatusChanges.Any())
            throw new InvalidOperationException("WorkOrder has already been initialized.");

        CurrentStatus = WorkOrderStatus.Received;

        StatusChanges.Add(new WorkOrderStatusChange
        {
            Id = Guid.NewGuid(),
            WorkOrderId = Id,
            FromStatus = null,
            ToStatus = WorkOrderStatus.Received,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = createdByUserId
        });
    }

    /// <summary>
    /// Recalcula TotalAmount sumando PriceSnapshot * Quantity de todos los servicios activos.
    /// </summary>
    public void RecalculateTotalAmount()
    {
        TotalAmount = Services
            .Where(s => !s.IsDeleted)
            .Sum(s => s.PriceSnapshot * s.Quantity);
    }
}
