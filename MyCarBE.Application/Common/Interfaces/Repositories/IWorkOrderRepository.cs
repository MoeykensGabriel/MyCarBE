using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    /// <summary>
    /// Full detail: Services (with Photos), general Photos, StatusChanges ordered by time.
    /// Used for the customer portal and admin detail view.
    /// </summary>
    Task<WorkOrder?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lightweight load that includes only Services — used when adding/removing services
    /// and recalculating the total amount.
    /// </summary>
    Task<WorkOrder?> GetWithServicesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkOrder>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkOrder>> GetByCustomerIdAtEntryAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkOrder>> GetByFleetIdAtEntryAsync(Guid fleetId, CancellationToken cancellationToken = default);
}
