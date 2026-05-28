using MediatR;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.Application.Features.VehicleTrips.Public.Queries.GetTripStation;

/// <summary>
/// Lo que ve el chofer al escanear el QR. Pública — no requiere auth.
/// El token actúa como credencial: si es válido, devuelve la info del auto.
/// </summary>
public record GetTripStationQuery(string Token) : IRequest<TripStationDto>;
