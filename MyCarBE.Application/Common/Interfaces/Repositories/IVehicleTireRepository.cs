using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IVehicleTireRepository : IRepository<VehicleTire>
{
    /// <summary>Cubierta activa para una posición específica del vehículo (o null si no hay).</summary>
    Task<VehicleTire?> GetActiveByPositionAsync(
        Guid vehicleId, TirePosition position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Todas las cubiertas de un vehículo (activas + históricas si includeReplaced=true),
    /// con las mediciones cargadas y ordenadas por fecha.
    /// </summary>
    Task<IReadOnlyList<VehicleTire>> GetByVehicleAsync(
        Guid vehicleId,
        bool includeReplaced = false,
        CancellationToken cancellationToken = default);

    /// <summary>Cubierta por id con sus mediciones cargadas.</summary>
    Task<VehicleTire?> GetByIdWithMeasurementsAsync(Guid id, CancellationToken cancellationToken = default);
}
