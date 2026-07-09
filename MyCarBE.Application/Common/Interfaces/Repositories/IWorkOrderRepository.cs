using MyCarBE.Application.Common.Models;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    /// <summary>
    /// Full detail: Services (with Photos), general Photos, StatusChanges ordered by time.
    /// </summary>
    Task<WorkOrder?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lightweight load that includes only Services — used when recalculating totals.
    /// </summary>
    Task<WorkOrder?> GetWithServicesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<WorkOrder>> GetAllPagedAsync(WorkOrderStatus? status, IReadOnlyList<WorkOrderStatus>? statuses, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<PagedResult<WorkOrder>> GetByVehicleIdPagedAsync(Guid vehicleId, WorkOrderStatus? status, IReadOnlyList<WorkOrderStatus>? statuses, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<PagedResult<WorkOrder>> GetByCustomerIdAtEntryPagedAsync(Guid customerId, WorkOrderStatus? status, IReadOnlyList<WorkOrderStatus>? statuses, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<PagedResult<WorkOrder>> GetByFleetIdAtEntryPagedAsync(Guid fleetId, WorkOrderStatus? status, IReadOnlyList<WorkOrderStatus>? statuses, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

    // ── WorkOrderService (línea de servicio) ─────────────────────────────────

    /// <summary>
    /// Obtiene un WorkOrderService con su WorkOrder y mecánico asignado.
    /// Usado por los endpoints de assign / accept / complete.
    /// </summary>
    Task<WorkOrderService?> GetServiceByIdAsync(Guid serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Servicios asignados a un mecánico filtrados por AssignmentStatus.
    /// Devuelve cada WorkOrderService con su WorkOrder y Vehicle (para mostrar contexto al mecánico).
    /// </summary>
    Task<IReadOnlyList<WorkOrderService>> GetServicesByMechanicAsync(
        Guid mechanicId,
        Domain.Enums.WorkOrderServiceAssignmentStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Órdenes en estado UnderInspection con Vehicle y los InspectionReports (solo AreaId)
    /// — usado por la query de "inspecciones pendientes" para mecánicos.
    /// </summary>
    Task<IReadOnlyList<WorkOrder>> GetUnderInspectionWithReportsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pool de trabajos disponibles para el mecánico: servicios Unassigned + Approved
    /// pertenecientes a WOs en InProgress. Incluye Vehicle y propietario para mostrar contexto.
    /// Ordenados por CreatedAt asc (FIFO — los más viejos primero).
    /// </summary>
    Task<IReadOnlyList<WorkOrderService>> GetAvailableServicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Servicios agendados que intersectan el rango [from, to]: ScheduledStart &lt;= to AND ScheduledEnd &gt;= from.
    /// Incluye WorkOrder.Vehicle, AssignedMechanic, Area. Ignora servicios sin scheduling.
    /// </summary>
    Task<IReadOnlyList<WorkOrderService>> GetScheduledServicesAsync(
        DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Órdenes agendadas (ScheduledStart/End) que intersectan [from, to] y ocupan bahía:
    /// estados post-aprobación (Approved, InProgress, Completed). Incluye Vehicle y dueño.
    /// Para el calendario de ocupación física por vehículo.
    /// </summary>
    Task<IReadOnlyList<WorkOrder>> GetScheduledWorkOrdersAsync(
        DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
