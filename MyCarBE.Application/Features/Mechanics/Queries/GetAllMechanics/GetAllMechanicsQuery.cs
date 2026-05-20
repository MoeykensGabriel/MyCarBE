using MediatR;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetAllMechanics;

public record GetAllMechanicsQuery(
    string? Search,
    bool    IncludeInactive,
    int     Page,
    int     PageSize
) : IRequest<PagedResult<MechanicDto>>;
