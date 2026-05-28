using MediatR;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.Application.Features.VehicleTrips.Public.Commands.EndTrip;

/// <summary>
/// El chofer escanea el QR al volver y cierra el viaje abierto con su km de llegada.
/// </summary>
public record EndTripCommand(string Token, int EndKm) : IRequest<VehicleTripDto>;
