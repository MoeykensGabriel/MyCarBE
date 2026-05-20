using MyCarBE.Application.Common.Models;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default);

    // --- Create checks (no exclusion) ---
    Task<bool> LicensePlateExistsAsync(string licensePlate, CancellationToken cancellationToken = default);
    Task<bool> VINExistsAsync(string vin, CancellationToken cancellationToken = default);

    // --- Update checks (exclude self) ---
    Task<bool> LicensePlateExistsAsync(string licensePlate, Guid excludeId, CancellationToken cancellationToken = default);
    Task<bool> VINExistsAsync(string vin, Guid excludeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Búsqueda paginada. search filtra por patente, marca o modelo (contains, case-insensitive).
    /// customerId y fleetId son opcionales — si se proveen, limitan al dueño.
    /// </summary>
    Task<PagedResult<Vehicle>> SearchPagedAsync(
        string? search, Guid? customerId, Guid? fleetId,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
