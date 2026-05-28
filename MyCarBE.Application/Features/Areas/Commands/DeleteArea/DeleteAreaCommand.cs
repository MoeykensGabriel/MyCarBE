using MediatR;

namespace MyCarBE.Application.Features.Areas.Commands.DeleteArea;

public record DeleteAreaCommand(Guid Id) : IRequest;
