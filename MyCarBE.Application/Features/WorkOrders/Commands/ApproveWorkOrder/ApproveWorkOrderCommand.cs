using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveWorkOrder;

/// <summary>
/// Aprobación del presupuesto vía token público (sin auth). El cliente selecciona qué
/// items aprueba — items no incluidos quedan Rejected (whitelist).
///
/// Reglas validadas en WorkOrder.ApplyCustomerApproval:
///   - Cada AlternativeGroupId requiere exactamente 1 item aprobado.
///   - Al menos 1 item aprobado en total.
///   - Los IDs deben pertenecer a items activos de la orden.
/// </summary>
public record ApproveWorkOrderCommand(
    string              Token,
    IReadOnlyList<Guid> ApprovedServiceIds,
    IReadOnlyList<Guid> ApprovedPartIds
) : IRequest<WorkOrderDetailDto>;
