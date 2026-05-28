using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderPart;

/// <summary>
/// Edita un repuesto cargado en una orden. Aplica solo durante Diagnosing y solo
/// si el repuesto no fue congelado al enviar el presupuesto (FrozenAt == null).
/// </summary>
public record UpdateWorkOrderPartCommand(
    Guid              WorkOrderId,
    Guid              PartId,
    string?           ProductCode,
    string            Name,
    decimal           UnitPrice,
    int               Quantity,
    WorkOrderPartTier Tier,
    Guid?             AlternativeGroupId
) : IRequest<WorkOrderDetailDto>;
