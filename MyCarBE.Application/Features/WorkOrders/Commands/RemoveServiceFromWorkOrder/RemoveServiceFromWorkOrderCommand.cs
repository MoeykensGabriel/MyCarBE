using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.RemoveServiceFromWorkOrder;

public record RemoveServiceFromWorkOrderCommand(
    Guid WorkOrderId,
    Guid WorkOrderServiceId
) : IRequest<WorkOrderDetailDto>;
