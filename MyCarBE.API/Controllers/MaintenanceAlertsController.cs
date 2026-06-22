using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.Maintenance.Commands.ResetMaintenanceAlert;
using MyCarBE.Application.Features.Maintenance.Commands.SetVehicleMaintenanceAlerts;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Application.Features.Maintenance.Queries.GetVehicleMaintenanceAlerts;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Alertas de mantenimiento configurables por vehículo: las define el recepcionista en el
/// ingreso y las edita/reinicia el admin desde la ficha. El customer las ve (cuando vencen)
/// vía /api/maintenance/summary.
/// </summary>
[ApiController]
[Authorize]
public class MaintenanceAlertsController : ControllerBase
{
    private readonly ISender _sender;
    public MaintenanceAlertsController(ISender sender) => _sender = sender;

    public record SetBody(IReadOnlyList<MaintenanceAlertItemInput> Items);

    /// <summary>
    /// Alertas configuradas de un vehículo (con su estado calculado). Accesible al dueño
    /// del vehículo además del taller: el acceso lo resuelve el handler por ownership.
    /// </summary>
    [HttpGet("api/vehicles/{vehicleId:guid}/maintenance-alerts")]
    [ProducesResponseType(typeof(IReadOnlyList<MaintenanceAlertConfigDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid vehicleId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetVehicleMaintenanceAlertsQuery(vehicleId), cancellationToken));

    /// <summary>Configura (set "replace") las alertas del vehículo. Solo taller.</summary>
    [HttpPut("api/vehicles/{vehicleId:guid}/maintenance-alerts")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(IReadOnlyList<MaintenanceAlertConfigDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Set(
        Guid vehicleId, [FromBody] SetBody body, CancellationToken cancellationToken)
        => Ok(await _sender.Send(
            new SetVehicleMaintenanceAlertsCommand(
                vehicleId, body.Items ?? Array.Empty<MaintenanceAlertItemInput>()),
            cancellationToken));

    /// <summary>Reinicia el ciclo de una alerta (se hizo el service). Solo taller.</summary>
    [HttpPost("api/vehicles/{vehicleId:guid}/maintenance-alerts/{alertId:guid}/reset")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(MaintenanceAlertConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reset(
        Guid vehicleId, Guid alertId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ResetMaintenanceAlertCommand(vehicleId, alertId), cancellationToken));
}
