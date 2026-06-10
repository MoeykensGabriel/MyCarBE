using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.VehicleOilServices.DTOs;
using MyCarBE.Application.Features.VehicleOilServices.Queries.GetOilServiceByVehicle;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Estado del aceite de un vehículo: último cambio + estimación del próximo service
/// (por km y por tiempo, lo que llegue primero).
///
/// La carga la hace el mecánico del área de aceite (o un generalista) dentro de la
/// inspección (CreateInspectionReport) — por eso acá solo exponemos la LECTURA.
///
/// Acceso: el handler usa VehicleOwnershipGuard (Admin / Customer dueño / Fleet Contact),
/// así que no hace falta restringir por rol en el endpoint.
/// </summary>
[ApiController]
[Authorize]
public class VehicleOilServicesController : ControllerBase
{
    private readonly ISender _sender;
    public VehicleOilServicesController(ISender sender) => _sender = sender;

    /// <summary>
    /// Devuelve el estado del aceite del vehículo (o 204 si nunca se registró un cambio).
    /// </summary>
    [HttpGet("api/vehicles/{vehicleId:guid}/oil-service")]
    [ProducesResponseType(typeof(VehicleOilServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetOilServiceByVehicleQuery(vehicleId), cancellationToken);
        return result is null ? NoContent() : Ok(result);
    }
}
