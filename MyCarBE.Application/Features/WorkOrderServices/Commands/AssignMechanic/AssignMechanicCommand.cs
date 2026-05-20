using MediatR;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.AssignMechanic;

public record AssignMechanicCommand(Guid WorkOrderServiceId, Guid MechanicId) : IRequest;
