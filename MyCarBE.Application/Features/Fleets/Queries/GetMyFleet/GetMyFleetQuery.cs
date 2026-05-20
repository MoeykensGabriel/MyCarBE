using MediatR;
using MyCarBE.Application.Features.Fleets.DTOs;

namespace MyCarBE.Application.Features.Fleets.Queries.GetMyFleet;

/// <summary>
/// Devuelve la flota del contacto autenticado usando el FleetId del JWT.
/// Usado por el portal del cliente de tipo flota.
/// </summary>
public record GetMyFleetQuery : IRequest<FleetDetailDto>;
