using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddServiceToWorkOrder;

/// <summary>
/// Adds a catalog service to a work order, snapshotting name, description and price
/// from the catalog at the moment of addition.
/// </summary>
public record AddServiceToWorkOrderCommand(
    Guid WorkOrderId,
    Guid CatalogServiceId,
    int  Quantity
) : IRequest<WorkOrderDetailDto>;
