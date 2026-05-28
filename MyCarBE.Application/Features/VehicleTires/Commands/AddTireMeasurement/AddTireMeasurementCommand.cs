using MediatR;
using MyCarBE.Application.Features.VehicleTires.DTOs;

namespace MyCarBE.Application.Features.VehicleTires.Commands.AddTireMeasurement;

public record AddTireMeasurementCommand(
    Guid     VehicleTireId,
    DateTime MeasuredOn,
    int      VehicleMileageAtMeasurement,
    decimal  InnerDepthMm,
    decimal  CenterDepthMm,
    decimal  OuterDepthMm,
    string?  Notes
) : IRequest<VehicleTireDto>;
