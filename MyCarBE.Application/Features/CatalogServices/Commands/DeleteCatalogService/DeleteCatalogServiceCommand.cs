using MediatR;

namespace MyCarBE.Application.Features.CatalogServices.Commands.DeleteCatalogService;

public record DeleteCatalogServiceCommand(Guid Id) : IRequest;
