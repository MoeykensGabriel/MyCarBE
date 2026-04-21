using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default);
    Task<bool> LicensePlateExistsAsync(string licensePlate, CancellationToken cancellationToken = default);
    Task<bool> VINExistsAsync(string vin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetByFleetIdAsync(Guid fleetId, CancellationToken cancellationToken = default);
}
