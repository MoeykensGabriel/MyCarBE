using MediatR;

namespace MyCarBE.Application.Features.Fleets.Commands.DeleteFleet;

public record DeleteFleetCommand(Guid Id) : IRequest;
