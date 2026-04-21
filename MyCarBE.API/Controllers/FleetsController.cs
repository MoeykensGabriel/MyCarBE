using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.Fleets.Commands.CreateFleet;
using MyCarBE.Application.Features.Fleets.Commands.DeleteFleet;
using MyCarBE.Application.Features.Fleets.Commands.UpdateFleet;
using MyCarBE.Application.Features.Fleets.DTOs;
using MyCarBE.Application.Features.Fleets.Queries.GetAllFleets;
using MyCarBE.Application.Features.Fleets.Queries.GetFleetById;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/fleets")]
[Authorize(Roles = "Admin")]
public class FleetsController : ControllerBase
{
    private readonly ISender _sender;

    public FleetsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retorna todas las flotas. Opcionalmente filtra por nombre de empresa o CUIT/RUC.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FleetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAllFleetsQuery(search), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle de una flota incluyendo sus contactos y vehículos.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FleetDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFleetByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva flota (empresa B2B). Retorna el Id de la flota creada.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateFleetCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Actualiza los datos de una flota existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FleetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFleetCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Elimina (soft delete) una flota.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFleetCommand(id), cancellationToken);
        return NoContent();
    }
}
