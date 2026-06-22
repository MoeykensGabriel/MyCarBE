using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddPartToWorkOrder;

public record AddPartToWorkOrderCommand(
    Guid              WorkOrderId,
    string?           ProductCode,
    string            Name,
    decimal           UnitPrice,
    int               Quantity,
    WorkOrderPartTier Tier
) : IRequest<WorkOrderDetailDto>;
