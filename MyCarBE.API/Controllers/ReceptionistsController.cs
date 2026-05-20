using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Receptionists.Commands.CreateReceptionist;
using MyCarBE.Application.Features.Receptionists.Commands.DeleteReceptionist;
using MyCarBE.Application.Features.Receptionists.Commands.UpdateReceptionist;
using MyCarBE.Application.Features.Receptionists.DTOs;
using MyCarBE.Application.Features.Receptionists.Queries.GetAllReceptionists;
using MyCarBE.Application.Features.Receptionists.Queries.GetReceptionistById;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/receptionists")]
[Authorize(Roles = "Admin")]
public class ReceptionistsController : ControllerBase
{
    private readonly ISender _sender;

    public ReceptionistsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReceptionistDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetAllReceptionistsQuery(search, includeInactive, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReceptionistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetReceptionistByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateReceptionistResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateReceptionistCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Receptionist.Id }, result);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ReceptionistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReceptionistCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id does not match body id.");
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteReceptionistCommand(id), cancellationToken);
        return NoContent();
    }
}
