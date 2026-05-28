using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddPartToWorkOrder;

/// <summary>
/// Agrega un repuesto a una WorkOrder durante la fase de cotización (Diagnosing).
/// ProductCode es opcional: si es null el repuesto es "custom" (compra suelta del taller)
/// y no se enviará al Sistema de Stock al aprobar el presupuesto.
/// </summary>
public record AddPartToWorkOrderCommand(
    Guid              WorkOrderId,
    string?           ProductCode,
    string            Name,
    decimal           UnitPrice,
    int               Quantity,
    WorkOrderPartTier Tier,
    /// <summary>
    /// Si tiene valor, este repuesto forma parte de un grupo de alternativas mutuamente
    /// excluyentes. Cliente debe elegir exactamente uno por grupo al aprobar.
    /// </summary>
    Guid?             AlternativeGroupId
) : IRequest<WorkOrderDetailDto>;
