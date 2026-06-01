using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderPart;

public record UpdateWorkOrderPartCommand(
    Guid              WorkOrderId,
    Guid              PartId,
    string?           ProductCode,
    string            Name,
    decimal           UnitPrice,
    decimal?          CustomerUnitPrice,
    int               Quantity,
    WorkOrderPartTier Tier,
    Guid?             AlternativeGroupId
) : IRequest<WorkOrderDetailDto>;
