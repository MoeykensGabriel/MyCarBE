using MediatR;
using MyCarBE.Application.Features.Fleets.DTOs;

namespace MyCarBE.Application.Features.Fleets.Queries.GetFleetById;

public record GetFleetByIdQuery(Guid Id) : IRequest<FleetDetailDto>;
