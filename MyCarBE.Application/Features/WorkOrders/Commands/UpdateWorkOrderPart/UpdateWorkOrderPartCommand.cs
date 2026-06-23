using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderPart;

public record UpdateWorkOrderPartCommand(
    Guid    WorkOrderId,
    Guid    PartId,
    string? ProductCode,
    string  Name,
    decimal UnitPrice,
    int     Quantity
) : IRequest<WorkOrderDetailDto>;
