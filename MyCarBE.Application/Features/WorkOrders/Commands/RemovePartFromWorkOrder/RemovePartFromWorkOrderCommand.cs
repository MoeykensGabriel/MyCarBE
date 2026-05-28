using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.RemovePartFromWorkOrder;

public record RemovePartFromWorkOrderCommand(
    Guid WorkOrderId,
    Guid PartId
) : IRequest<WorkOrderDetailDto>;
