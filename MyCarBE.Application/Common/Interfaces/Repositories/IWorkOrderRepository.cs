using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    /// <summary>
    /// Trae la orden con Services, Photos y StatusChanges incluidos (para el portal del cliente).
    /// </summary>
    Task<WorkOrder?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkOrder>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkOrder>> GetByCustomerIdAtEntryAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkOrder>> GetByFleetIdAtEntryAsync(Guid fleetId, CancellationToken cancellationToken = default);
}
