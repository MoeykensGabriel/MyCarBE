using MediatR;
using MyCarBE.Application.Features.Vehicles.DTOs;

namespace MyCarBE.Application.Features.Vehicles.Queries.GetVehiclesByOwner;

/// <summary>
/// Returns all vehicles for a given owner.
/// Exactly one of CustomerId / FleetId must be provided.
/// </summary>
public record GetVehiclesByOwnerQuery(
    Guid? CustomerId,
    Guid? FleetId
) : IRequest<IReadOnlyList<VehicleDto>>;
