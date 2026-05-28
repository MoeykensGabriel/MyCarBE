using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleTrips.DTOs;

public record VehicleTripDto(
    Guid               Id,
    Guid               VehicleId,
    string             VehicleLicensePlate,
    string             VehicleBrand,
    string             VehicleModel,
    string             DriverName,
    string             DriverDocument,
    int                StartKm,
    int?               EndKm,
    DateTime           StartedAt,
    DateTime?          EndedAt,
    VehicleTripStatus  Status
);
