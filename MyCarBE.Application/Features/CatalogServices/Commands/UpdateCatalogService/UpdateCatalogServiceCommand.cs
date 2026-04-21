using MediatR;
using MyCarBE.Application.Features.CatalogServices.DTOs;

namespace MyCarBE.Application.Features.CatalogServices.Commands.UpdateCatalogService;

public record UpdateCatalogServiceCommand(
    Guid    Id,
    string  Name,
    string  Description,
    decimal DefaultPrice,
    int     EstimatedDurationMinutes,
    bool    IsActive
) : IRequest<CatalogServiceDto>;
