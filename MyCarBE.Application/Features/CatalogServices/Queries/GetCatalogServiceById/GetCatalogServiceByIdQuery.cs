using MediatR;
using MyCarBE.Application.Features.CatalogServices.DTOs;

namespace MyCarBE.Application.Features.CatalogServices.Queries.GetCatalogServiceById;

public record GetCatalogServiceByIdQuery(Guid Id) : IRequest<CatalogServiceDto>;
