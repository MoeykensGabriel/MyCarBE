using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.DecideAdditionalItems;

/// <summary>
/// Registra la decisión del cliente sobre items ADICIONALES: trabajo que surgió después de
/// aprobar el presupuesto original (la orden está Approved o InProgress y NO retrocede).
///
/// La oficina carga la decisión después de consultarlo con el cliente (teléfono/WhatsApp).
/// A diferencia de la aprobación original (whitelist total), acá se decide item por item:
/// los Pending no mencionados siguen Pending y se pueden decidir más adelante.
///
/// Side effects:
///   - Items decididos pasan a Approved/Rejected; el total se recalcula (los Pending
///     adicionales no suman hasta ser aprobados).
///   - Si se aprobaron repuestos con ProductCode, se genera un pedido adicional al
///     depósito con SOLO esos repuestos (delta — los ya pedidos no se repiten).
/// </summary>
public record DecideAdditionalItemsCommand(
    Guid WorkOrderId,
    IReadOnlyList<Guid> ApprovedServiceIds,
    IReadOnlyList<Guid> RejectedServiceIds,
    IReadOnlyList<Guid> ApprovedPartIds,
    IReadOnlyList<Guid> RejectedPartIds) : IRequest<WorkOrderDetailDto>;
