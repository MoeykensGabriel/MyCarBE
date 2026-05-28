using MediatR;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.Application.Features.VehicleTrips.Admin.Commands.CloseTripManually;

/// <summary>
/// El encargado cierra un viaje abierto cuando el chofer se olvidó de escanear al volver.
/// </summary>
public record CloseTripManuallyCommand(Guid TripId, int EndKm) : IRequest<VehicleTripDto>;
