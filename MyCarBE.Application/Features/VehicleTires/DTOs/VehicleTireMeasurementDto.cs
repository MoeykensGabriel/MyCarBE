namespace MyCarBE.Application.Features.VehicleTires.DTOs;

public record VehicleTireMeasurementDto(
    Guid     Id,
    Guid     VehicleTireId,
    DateTime MeasuredOn,
    int      VehicleMileageAtMeasurement,
    decimal  InnerDepthMm,
    decimal  CenterDepthMm,
    decimal  OuterDepthMm,
    decimal  AverageDepthMm,
    decimal  SpreadMm,
    string?  Notes,
    Guid?    MeasuredByUserId,
    DateTime CreatedAt
);
