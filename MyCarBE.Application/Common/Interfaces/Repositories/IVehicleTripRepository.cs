using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IVehicleTripRepository : IRepository<VehicleTrip>
{
    /// <summary>Vehículo por su TripToken (para la estación pública del chofer).</summary>
    Task<Vehicle?> GetVehicleByTripTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Viaje abierto (Open) actual de un vehículo. Null si no hay.</summary>
    Task<VehicleTrip?> GetOpenTripForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>Último viaje cerrado del vehículo — útil para sugerir el km de inicio.</summary>
    Task<VehicleTrip?> GetLastClosedTripForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>Historial de viajes de un vehículo (orden descendente).</summary>
    Task<IReadOnlyList<VehicleTrip>> GetTripsByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>Viajes abiertos de toda una flota — para el panel del encargado.</summary>
    Task<IReadOnlyList<VehicleTrip>> GetOpenTripsByFleetAsync(Guid fleetId, CancellationToken cancellationToken = default);

    /// <summary>True si ya existe un vehículo con ese TripToken (para evitar colisiones).</summary>
    Task<bool> TripTokenExistsAsync(string token, CancellationToken cancellationToken = default);
}
