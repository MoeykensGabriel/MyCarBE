using MediatR;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.Application.Features.VehicleTrips.Public.Commands.StartTrip;

/// <summary>
/// El chofer escanea el QR y carga su salida. Si ya había un viaje abierto (porque el chofer
/// anterior se olvidó de cerrarlo), el sistema lo cierra automáticamente como AutoClosed
/// con endKm = startKm del nuevo viaje.
/// </summary>
public record StartTripCommand(
    string Token,
    string DriverName,
    string DriverDocument,
    int    StartKm
) : IRequest<VehicleTripDto>;
