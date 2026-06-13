using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Application.Features.Maintenance.Queries.GetMaintenanceSummary;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Resumen de mantenimiento para el Inicio del cliente: junta las alertas de
/// todos sus vehículos (hoy cubiertas; aceite y batería se suman después). El
/// dueño se deriva del JWT, así que el scope es el control de acceso.
/// </summary>
[ApiController]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly ISender _sender;
    public MaintenanceController(ISender sender) => _sender = sender;

    /// <summary>Alertas de mantenimiento de los vehículos del cliente actual.</summary>
    [HttpGet("api/maintenance/summary")]
    [ProducesResponseType(typeof(IReadOnlyList<MaintenanceAlertDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaintenanceSummaryQuery(), cancellationToken);
        return Ok(result);
    }
}
