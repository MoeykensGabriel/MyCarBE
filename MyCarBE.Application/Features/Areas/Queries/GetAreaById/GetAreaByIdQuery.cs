using MediatR;
using MyCarBE.Application.Features.Areas.DTOs;

namespace MyCarBE.Application.Features.Areas.Queries.GetAreaById;

public record GetAreaByIdQuery(Guid Id) : IRequest<AreaDto>;
