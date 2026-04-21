using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrders;

/// <summary>
/// Returns a summary list filtered by one of: vehicleId, customerId (at entry), fleetId (at entry).
/// Exactly one filter should be provided; if none, returns an empty list.
/// </summary>
public record GetWorkOrdersQuery(
    Guid? VehicleId,
    Guid? CustomerId,
    Guid? FleetId
) : IRequest<IReadOnlyList<WorkOrderSummaryDto>>;
