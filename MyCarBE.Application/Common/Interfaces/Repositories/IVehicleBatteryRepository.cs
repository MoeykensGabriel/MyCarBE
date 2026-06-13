using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IVehicleBatteryRepository : IRepository<VehicleBattery>
{
    /// <summary>Batería activa del vehículo (o null si no hay).</summary>
    Task<VehicleBattery?> GetActiveByVehicleAsync(
        Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Baterías de un vehículo (activa + históricas si includeReplaced=true),
    /// con los chequeos cargados y ordenados por fecha.
    /// </summary>
    Task<IReadOnlyList<VehicleBattery>> GetByVehicleAsync(
        Guid vehicleId,
        bool includeReplaced = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Baterías activas de todos los vehículos de un dueño (customer XOR fleet), con sus
    /// chequeos y el vehículo cargados. Una sola query para el resumen de mantenimiento.
    /// Si ambos ids son null, devuelve vacío.
    /// </summary>
    Task<IReadOnlyList<VehicleBattery>> GetActiveByOwnerAsync(
        Guid? customerId, Guid? fleetId, CancellationToken cancellationToken = default);
}
