using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleTires.DTOs;

public record VehicleTireDto(
    Guid          Id,
    Guid          VehicleId,
    TirePosition  Position,
    string        Brand,
    string        Model,
    string        SizeSpec,
    DateOnly      InstalledOn,
    int           InstalledAtKm,
    decimal       InitialTreadDepthMm,
    int           ExpectedLifeKm,
    bool          IsActive,
    DateOnly?     ReplacedOn,
    int?          ReplacedAtKm,
    DateTime      CreatedAt,
    DateTime      UpdatedAt,
    IReadOnlyList<VehicleTireMeasurementDto> Measurements,
    TireWearEstimationDto                    Estimation
);
