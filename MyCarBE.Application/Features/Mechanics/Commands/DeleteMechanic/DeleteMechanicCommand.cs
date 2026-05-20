using MediatR;

namespace MyCarBE.Application.Features.Mechanics.Commands.DeleteMechanic;

public record DeleteMechanicCommand(Guid Id) : IRequest;
