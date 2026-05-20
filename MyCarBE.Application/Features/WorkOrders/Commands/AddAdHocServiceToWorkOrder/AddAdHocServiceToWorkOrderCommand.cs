using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddAdHocServiceToWorkOrder;

/// <summary>
/// Agrega un servicio "puntual" a una WorkOrder. No requiere que exista en el
/// catálogo — los datos viven solo en los snapshots del WorkOrderService.
/// Útil para trabajos únicos (ej: "Soldadura de soporte de escape") que no
/// tiene sentido sumar al catálogo permanente.
/// </summary>
public record AddAdHocServiceToWorkOrderCommand(
    Guid    WorkOrderId,
    string  Name,
    string  Description,
    decimal Price,
    int     EstimatedDurationMinutes,
    int     Quantity
) : IRequest<WorkOrderDetailDto>;
