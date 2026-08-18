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
    /// <param name="mileageStaleBefore">
    /// Si viene, deja solo los vehículos cuya última lectura de kilometraje es anterior a esa
    /// fecha, o que nunca tuvieron una. La fecha la calcula el handler contra el umbral del
    /// taller: el repositorio filtra, no decide qué es "vencido".
    ///
    /// Va acá y no en memoria porque el listado está paginado — filtrar después de traer la
    /// página dejaría afuera a los vencidos de las páginas siguientes, que es justo lo que
    /// pasa con una flota grande.
    /// </param>
    Task<PagedResult<Vehicle>> SearchPagedAsync(
        string? search, Guid? customerId, Guid? fleetId,
        int page, int pageSize, string? sort = null,
        DateTime? mileageStaleBefore = null, CancellationToken cancellationToken = default);
}
