using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Mechanics.Commands.AssignAreasToMechanic;
using MyCarBE.Application.Features.Mechanics.Commands.CreateMechanic;
using MyCarBE.Application.Features.Mechanics.Commands.DeleteMechanic;
using MyCarBE.Application.Features.Mechanics.Commands.UpdateMechanic;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Application.Features.Mechanics.Queries.GetAllMechanics;
using MyCarBE.Application.Features.Mechanics.Queries.GetMechanicById;
using MyCarBE.Application.Features.Mechanics.Queries.GetMyAvailableServices;
using MyCarBE.Application.Features.Mechanics.Queries.GetMyPendingInspections;
using MyCarBE.Application.Features.Mechanics.Queries.GetMyProfile;
using MyCarBE.Application.Features.Mechanics.Queries.GetMyTasks;
using MyCarBE.Domain.Enums;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/mechanics")]
[Authorize]
public class MechanicsController : ControllerBase
{
    private readonly ISender _sender;

    public MechanicsController(ISender sender) => _sender = sender;

    // ── Gestión (Admin) ──────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<MechanicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetAllMechanicsQuery(search, includeInactive, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MechanicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMechanicByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CreateMechanicResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateMechanicCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Mechanic.Id }, result);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MechanicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMechanicCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id does not match body id.");
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteMechanicCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sincroniza las áreas de especialidad de un mecánico (PUT semantics: reemplazo total).
    /// Pasar lista vacía deja al mecánico sin áreas asignadas.
    /// </summary>
    public record AssignAreasBody(IReadOnlyList<Guid> AreaIds);

    [HttpPut("{id:guid}/areas")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MechanicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignAreas(
        Guid id,
        [FromBody] AssignAreasBody body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AssignAreasToMechanicCommand(id, body.AreaIds ?? Array.Empty<Guid>()),
            cancellationToken);
        return Ok(result);
    }

    // ── Self-service (Mechanic) ──────────────────────────────────────────────

    [HttpGet("me")]
    [Authorize(Roles = "Mechanic")]
    [ProducesResponseType(typeof(MechanicDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyProfileQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("me/tasks")]
    [Authorize(Roles = "Mechanic")]
    [ProducesResponseType(typeof(IReadOnlyList<MechanicTaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTasks(
        [FromQuery] WorkOrderServiceAssignmentStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyTasksQuery(status), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Órdenes en fase de inspección con áreas pendientes que le corresponden al
    /// mecánico logueado. Solo aparecen áreas activas (que el mecánico tiene asignadas)
    /// y que aún no fueron reportadas o marcadas "sin hallazgos" en esa orden.
    /// </summary>
    [HttpGet("me/pending-inspections")]
    [Authorize(Roles = "Mechanic")]
    [ProducesResponseType(typeof(IReadOnlyList<PendingInspectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPendingInspections(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyPendingInspectionsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Pool de trabajos disponibles para auto-tomar: servicios Unassigned + Approved
    /// de WOs en InProgress. El mecánico hace claim desde la pantalla "/mecanico/disponibles".
    /// Ordenados FIFO (los más viejos primero).
    /// </summary>
    [HttpGet("me/available-services")]
    [Authorize(Roles = "Mechanic")]
    [ProducesResponseType(typeof(IReadOnlyList<AvailableServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAvailableServices(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyAvailableServicesQuery(), cancellationToken);
        return Ok(result);
    }
}
