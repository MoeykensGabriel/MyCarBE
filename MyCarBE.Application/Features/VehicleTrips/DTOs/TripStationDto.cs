namespace MyCarBE.Application.Features.VehicleTrips.DTOs;

/// <summary>
/// Lo que ve el chofer al escanear el QR: info mínima del vehículo + estado del viaje
/// (si hay uno abierto o no) + último km conocido como sugerencia.
/// </summary>
public record TripStationDto(
    Guid    VehicleId,
    string  LicensePlate,
    string  Brand,
    string  Model,
    /// <summary>Último km conocido (sea por viaje cerrado o por currentMileage del vehículo).</summary>
    int     LastKnownKm,
    /// <summary>Si hay un viaje abierto, sus datos. Si no, null.</summary>
    VehicleTripDto? OpenTrip
);
