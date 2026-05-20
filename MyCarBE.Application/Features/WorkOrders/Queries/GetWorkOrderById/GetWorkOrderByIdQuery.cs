using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderById;

public record GetWorkOrderByIdQuery(Guid Id) : IRequest<WorkOrderDetailDto>;
