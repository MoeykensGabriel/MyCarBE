using MediatR;
using MyCarBE.Application.Features.Areas.DTOs;

namespace MyCarBE.Application.Features.Areas.Queries.GetAllAreas;

public record GetAllAreasQuery(bool IncludeInactive) : IRequest<IReadOnlyList<AreaDto>>;
