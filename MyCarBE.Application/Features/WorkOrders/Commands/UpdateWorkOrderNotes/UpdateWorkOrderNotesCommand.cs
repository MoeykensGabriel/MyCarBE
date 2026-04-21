using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderNotes;

public record UpdateWorkOrderNotesCommand(
    Guid    WorkOrderId,
    string? CustomerNote,
    string? TechnicianNote
) : IRequest<WorkOrderSummaryDto>;
