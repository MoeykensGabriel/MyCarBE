using MediatR;
using MyCarBE.Application.Features.Vehicles.DTOs;

namespace MyCarBE.Application.Features.Vehicles.Queries.GetVehicleById;

public record GetVehicleByIdQuery(Guid Id) : IRequest<VehicleDto>;
