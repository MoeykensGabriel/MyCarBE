using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.Dashboard.DTOs;
using MyCarBE.Application.Features.Dashboard.Queries.GetDashboardSummary;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Resumen ejecutivo para el panel de administración.
    /// Incluye conteos por estado, ingresos del día y mes, y las últimas 5 órdenes.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDashboardSummaryQuery(), cancellationToken);
        return Ok(result);
    }
}
