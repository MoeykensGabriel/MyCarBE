using MediatR;
using MyCarBE.Application.Features.Fleets.DTOs;

namespace MyCarBE.Application.Features.Fleets.Queries.GetAllFleets;

/// <param name="SearchTerm">Optional. Filters by company name or TaxId.</param>
public record GetAllFleetsQuery(string? SearchTerm = null) : IRequest<IReadOnlyList<FleetDto>>;
