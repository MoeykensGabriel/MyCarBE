using MediatR;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.AcceptService;

public record AcceptServiceCommand(Guid WorkOrderServiceId) : IRequest;
