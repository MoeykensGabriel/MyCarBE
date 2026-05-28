using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.VehicleTrips.DTOs;
using MyCarBE.Application.Features.VehicleTrips.Public.Commands.EndTrip;
using MyCarBE.Application.Features.VehicleTrips.Public.Commands.StartTrip;
using MyCarBE.Application.Features.VehicleTrips.Public.Queries.GetTripStation;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Endpoints PÚBLICOS (sin autenticación) que usa el chofer al escanear el QR
/// pegado adentro del vehículo. El TripToken funciona como credencial: si es válido,
/// permite registrar entrada/salida.
///
/// Rate-limit y abuso: lo cubre el firewall / front (no es código de esta capa).
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/trip-stations")]
public class TripStationsController : ControllerBase
{
    private readonly ISender _sender;
    public TripStationsController(ISender sender) => _sender = sender;

    public record StartBody(string DriverName, string DriverDocument, int StartKm);
    public record EndBody(int EndKm);

    /// <summary>Info del vehículo + viaje abierto si hay + último km conocido.</summary>
    [HttpGet("{token}")]
    [ProducesResponseType(typeof(TripStationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string token, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTripStationQuery(token), cancellationToken);
        return Ok(result);
    }

    /// <summary>Abre un viaje. Si había uno abierto, lo cierra automáticamente.</summary>
    [HttpPost("{token}/start")]
    [ProducesResponseType(typeof(VehicleTripDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(string token, [FromBody] StartBody body, CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(new StartTripCommand(
            token, body.DriverName, body.DriverDocument, body.StartKm), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    /// <summary>Cierra el viaje abierto actual con el km de llegada.</summary>
    [HttpPost("{token}/end")]
    [ProducesResponseType(typeof(VehicleTripDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> End(string token, [FromBody] EndBody body, CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(new EndTripCommand(token, body.EndKm), cancellationToken);
        return Ok(dto);
    }
}
