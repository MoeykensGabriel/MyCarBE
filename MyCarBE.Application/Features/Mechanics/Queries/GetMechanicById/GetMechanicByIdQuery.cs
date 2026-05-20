using MediatR;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMechanicById;

public record GetMechanicByIdQuery(Guid Id) : IRequest<MechanicDto>;
