using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveAsCustomer;

/// <summary>
/// Aprobación del presupuesto desde el panel del cliente logueado (sin token).
/// Mismo contrato item-by-item que ApproveWorkOrderCommand pero con auth por JWT en vez
/// de por token. El handler valida ownership: el currentUser debe ser dueño de la WO
/// (CustomerIdAtEntry == currentUser.CustomerId  o  FleetIdAtEntry == currentUser.FleetId).
/// </summary>
public record ApproveAsCustomerCommand(
    Guid                WorkOrderId,
    IReadOnlyList<Guid> ApprovedServiceIds,
    IReadOnlyList<Guid> ApprovedPartIds
) : IRequest<WorkOrderDetailDto>;
