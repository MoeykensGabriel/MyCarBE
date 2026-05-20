using MediatR;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrders;

public record GetWorkOrdersQuery(
    Guid?                  VehicleId,
    Guid?                  CustomerId,
    Guid?                  FleetId,
    WorkOrderStatus?       Status    = null,
    WorkOrderOwnerType?    OwnerType = null,
    string?                Search    = null,
    int                    Page      = 1,
    int                    PageSize  = 20
) : IRequest<PagedResult<WorkOrderSummaryDto>>;
