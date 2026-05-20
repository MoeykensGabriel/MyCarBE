using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.Settings.Commands.UpdateWorkshopSettings;
using MyCarBE.Application.Features.Settings.DTOs;
using MyCarBE.Application.Features.Settings.Queries.GetWorkshopSettings;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Endpoints de configuración global. Solo Admin puede leer y editar.
/// </summary>
[ApiController]
[Route("api/settings")]
[Authorize(Roles = "Admin")]
public class SettingsController : ControllerBase
{
    private readonly ISender _sender;

    public SettingsController(ISender sender) => _sender = sender;

    /// <summary>Devuelve la configuración del taller.</summary>
    [HttpGet("workshop")]
    [ProducesResponseType(typeof(WorkshopSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkshop(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkshopSettingsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Actualiza la configuración del taller.</summary>
    [HttpPut("workshop")]
    [ProducesResponseType(typeof(WorkshopSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateWorkshop(
        [FromBody] UpdateWorkshopSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}
