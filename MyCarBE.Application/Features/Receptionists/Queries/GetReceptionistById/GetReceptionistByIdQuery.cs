using MediatR;
using MyCarBE.Application.Features.Receptionists.DTOs;

namespace MyCarBE.Application.Features.Receptionists.Queries.GetReceptionistById;

public record GetReceptionistByIdQuery(Guid Id) : IRequest<ReceptionistDto>;
