using MediatR;
using MyCarBE.Application.Features.CatalogServices.DTOs;

namespace MyCarBE.Application.Features.CatalogServices.Queries.GetAllCatalogServices;

/// <param name="IncludeInactive">Solo Admin puede pasarlo en true para ver servicios inactivos.</param>
public record GetAllCatalogServicesQuery(bool IncludeInactive = false) : IRequest<IReadOnlyList<CatalogServiceDto>>;
