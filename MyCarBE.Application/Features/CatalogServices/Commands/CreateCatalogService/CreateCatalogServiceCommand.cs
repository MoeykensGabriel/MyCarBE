using MediatR;

namespace MyCarBE.Application.Features.CatalogServices.Commands.CreateCatalogService;

public record CreateCatalogServiceCommand(
    string Name,
    string Description,
    decimal DefaultPrice,
    int EstimatedDurationMinutes
) : IRequest<Guid>;
