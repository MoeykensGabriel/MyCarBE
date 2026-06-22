using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderServicePrice;

/// <summary>
/// Edita el precio de venta de un servicio de la orden (precio único modificable, lo fija el
/// admin consultándolo por fuera). Solo en Diagnosing y si el servicio no está congelado.
/// </summary>
public record UpdateWorkOrderServicePriceCommand(
    Guid    WorkOrderId,
    Guid    ServiceId,
    decimal Price
) : IRequest<WorkOrderDetailDto>;
