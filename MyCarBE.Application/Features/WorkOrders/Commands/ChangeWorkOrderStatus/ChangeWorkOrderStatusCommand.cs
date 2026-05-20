using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ChangeWorkOrderStatus;

/// <summary>
/// Advances or cancels a work order following the defined state machine.
/// Note is required when NewStatus is Cancelled.
/// </summary>
public record ChangeWorkOrderStatusCommand(
    Guid            WorkOrderId,
    WorkOrderStatus NewStatus,
    string?         Note
) : IRequest<WorkOrderDetailDto>;
