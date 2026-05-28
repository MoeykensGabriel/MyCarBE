using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.VehicleTrips.Admin.Commands.CloseTripManually;
using MyCarBE.Application.Features.VehicleTrips.Admin.Commands.RegenerateTripToken;
using MyCarBE.Application.Features.VehicleTrips.Admin.Queries.GetOpenTripsForMyFleet;
using MyCarBE.Application.Features.VehicleTrips.Admin.Queries.GetTripsByVehicle;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Endpoints privados de gestión de viajes — los usa el encargado de flota (Customer con FleetId)
/// y el Admin. La autorización fina (¿es de mi flota?) la valida el handler via VehicleOwnershipGuard.
/// </summary>
[ApiController]
[Authorize]
public class VehicleTripsController : ControllerBase
{
    private readonly ISender _sender;
    public VehicleTripsController(ISender sender) => _sender = sender;

    public record CloseBody(int EndKm);
    public record RegenerateResponse(string Token);

    /// <summary>Genera o regenera el TripToken (QR) del vehículo.</summary>
    [HttpPost("api/vehicles/{vehicleId:guid}/trip-token/regenerate")]
    [ProducesResponseType(typeof(RegenerateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Regenerate(Guid vehicleId, CancellationToken cancellationToken)
    {
        var token = await _sender.Send(new RegenerateTripTokenCommand(vehicleId), cancellationToken);
        return Ok(new RegenerateResponse(token));
    }

    /// <summary>Historial de viajes de un vehículo (orden descendente por StartedAt).</summary>
    [HttpGet("api/vehicles/{vehicleId:guid}/trips")]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleTripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListByVehicle(Guid vehicleId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTripsByVehicleQuery(vehicleId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Viajes abiertos de toda la flota del usuario actual.</summary>
    [HttpGet("api/fleets/mine/open-trips")]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleTripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> OpenForMyFleet(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOpenTripsForMyFleetQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Cierre manual de un viaje abierto (el chofer se olvidó de escanear al volver).</summary>
    [HttpPost("api/trips/{id:guid}/close")]
    [ProducesResponseType(typeof(VehicleTripDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseBody body, CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(new CloseTripManuallyCommand(id, body.EndKm), cancellationToken);
        return Ok(dto);
    }
}
