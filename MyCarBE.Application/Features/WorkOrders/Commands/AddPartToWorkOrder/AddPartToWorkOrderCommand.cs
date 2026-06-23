using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddPartToWorkOrder;

public record AddPartToWorkOrderCommand(
    Guid    WorkOrderId,
    string? ProductCode,
    string  Name,
    decimal UnitPrice,
    int     Quantity
) : IRequest<WorkOrderDetailDto>;
