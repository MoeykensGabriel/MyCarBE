using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.VehicleMileage.Commands.ReportVehicleMileage;
using MyCarBE.Application.Features.VehicleMileage.DTOs;
using MyCarBE.Application.Features.VehicleMileage.Queries.GetVehicleMileageReadings;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Lecturas de kilometraje del vehículo: carga periódica del cliente + historial
/// de trazabilidad. El acceso lo resuelve VehicleOwnershipGuard en los handlers
/// (Admin / Customer dueño / contacto de flota), así que basta [Authorize].
/// </summary>
[ApiController]
[Authorize]
public class VehicleMileageController : ControllerBase
{
    private readonly ISender _sender;
    public VehicleMileageController(ISender sender) => _sender = sender;

    public record ReportMileageBody(int Mileage);

    /// <summary>Registra una lectura nueva del odómetro declarada por el usuario.</summary>
    [HttpPost("api/vehicles/{vehicleId:guid}/mileage-readings")]
    [ProducesResponseType(typeof(VehicleMileageReadingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Report(
        Guid vehicleId,
        [FromBody] ReportMileageBody body,
        CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(
            new ReportVehicleMileageCommand(vehicleId, body.Mileage), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    /// <summary>Historial de lecturas (las más recientes primero, máx. 50).</summary>
    [HttpGet("api/vehicles/{vehicleId:guid}/mileage-readings")]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleMileageReadingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(
        Guid vehicleId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetVehicleMileageReadingsQuery(vehicleId), cancellationToken);
        return Ok(result);
    }
}
