using MediatR;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.CompleteService;

public record CompleteServiceCommand(
    Guid    WorkOrderServiceId,
    string  Notes,
    string? Findings
) : IRequest;
