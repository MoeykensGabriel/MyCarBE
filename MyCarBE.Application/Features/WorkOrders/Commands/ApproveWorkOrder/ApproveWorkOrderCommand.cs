using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveWorkOrder;

public record ApproveWorkOrderCommand(string Token) : IRequest<WorkOrderDetailDto>;
