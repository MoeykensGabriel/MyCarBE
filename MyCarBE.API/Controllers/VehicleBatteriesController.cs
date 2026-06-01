using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.VehicleBatteries.DTOs;
using MyCarBE.Application.Features.VehicleBatteries.Queries.GetBatteryByVehicle;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Batería de un vehículo y su historial de chequeos de estado.
///
/// La carga la hace el mecánico del área de batería dentro de la inspección
/// (CreateInspectionReport) — por eso acá solo exponemos la LECTURA.
///
/// Acceso: el handler usa VehicleOwnershipGuard (Admin / Customer dueño / Fleet Contact),
/// así que no hace falta restringir por rol en el endpoint.
/// </summary>
[ApiController]
[Authorize]
public class VehicleBatteriesController : ControllerBase
{
    private readonly ISender _sender;
    public VehicleBatteriesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Devuelve la batería activa del vehículo con su historial de chequeos (o 204 si no hay).
    /// Pasar includeReplaced=true para considerar también baterías reemplazadas.
    /// </summary>
    [HttpGet("api/vehicles/{vehicleId:guid}/battery")]
    [ProducesResponseType(typeof(VehicleBatteryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        Guid vehicleId,
        [FromQuery] bool includeReplaced = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetBatteryByVehicleQuery(vehicleId, includeReplaced), cancellationToken);

        return result is null ? NoContent() : Ok(result);
    }
}
