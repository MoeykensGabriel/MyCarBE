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

    Task<PagedResult<WorkOrder>> GetAllPagedAsync(WorkOrderStatus? status, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<WorkOrder>> GetByVehicleIdPagedAsync(Guid vehicleId, WorkOrderStatus? status, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<WorkOrder>> GetByCustomerIdAtEntryPagedAsync(Guid customerId, WorkOrderStatus? status, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<WorkOrder>> GetByFleetIdAtEntryPagedAsync(Guid fleetId, WorkOrderStatus? status, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, CancellationToken cancellationToken = default);

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
}
