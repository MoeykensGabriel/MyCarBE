using MediatR;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.Application.Features.VehicleTrips.Admin.Queries.GetTripsByVehicle;

public record GetTripsByVehicleQuery(Guid VehicleId)
    : IRequest<IReadOnlyList<VehicleTripDto>>;
