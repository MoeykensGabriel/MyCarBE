using MediatR;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.Application.Features.VehicleTrips.Admin.Queries.GetOpenTripsForMyFleet;

/// <summary>
/// Viajes abiertos de la flota del usuario actual (encargado). Para el panel /my-fleet.
/// </summary>
public record GetOpenTripsForMyFleetQuery : IRequest<IReadOnlyList<VehicleTripDto>>;
