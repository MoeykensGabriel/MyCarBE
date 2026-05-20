using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveAsCustomer;

/// <summary>
/// Aprobación del presupuesto desde el panel del cliente logueado (sin token).
/// El handler valida que el usuario actual sea el dueño de la WorkOrder
/// (CustomerIdAtEntry == currentUser.CustomerId  o
///  FleetIdAtEntry == currentUser.FleetId).
/// </summary>
public record ApproveAsCustomerCommand(Guid WorkOrderId) : IRequest<WorkOrderDetailDto>;
