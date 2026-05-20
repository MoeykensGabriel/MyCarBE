using MediatR;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Receptionists.DTOs;

namespace MyCarBE.Application.Features.Receptionists.Queries.GetAllReceptionists;

public record GetAllReceptionistsQuery(
    string? Search,
    bool    IncludeInactive,
    int     Page,
    int     PageSize
) : IRequest<PagedResult<ReceptionistDto>>;
