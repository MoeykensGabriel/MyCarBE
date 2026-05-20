using MediatR;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.UnassignMechanic;

public record UnassignMechanicCommand(Guid WorkOrderServiceId) : IRequest;
