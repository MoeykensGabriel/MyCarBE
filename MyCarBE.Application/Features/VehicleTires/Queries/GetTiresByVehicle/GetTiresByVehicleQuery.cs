using MediatR;
using MyCarBE.Application.Features.VehicleTires.DTOs;

namespace MyCarBE.Application.Features.VehicleTires.Queries.GetTiresByVehicle;

public record GetTiresByVehicleQuery(
    Guid VehicleId,
    bool IncludeReplaced = false
) : IRequest<IReadOnlyList<VehicleTireDto>>;
