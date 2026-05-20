using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.Vehicles.Commands.CreateVehicle;
using MyCarBE.Application.Features.Vehicles.Commands.DeleteVehicle;
using MyCarBE.Application.Features.Vehicles.Commands.UpdateVehicle;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Vehicles.DTOs;
using MyCarBE.Application.Features.Vehicles.Queries.GetVehicleById;
using MyCarBE.Application.Features.Vehicles.Queries.GetVehiclesByOwner;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly ISender _sender;

    public VehiclesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Búsqueda paginada de vehículos.
    /// Admin: search por patente/marca/modelo, opcionalmente filtrado por customerId o fleetId.
    /// Customer: ignora filtros — ve solo sus propios vehículos; search filtra dentro de los suyos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid?   customerId,
        [FromQuery] Guid?   fleetId,
        [FromQuery] string? search,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        CancellationToken   cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetVehiclesByOwnerQuery(customerId, fleetId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna un vehículo por Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetVehicleByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra un nuevo vehículo. Debe pertenecer a un Cliente o a una Flota (XOR). Solo Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Actualiza un vehículo existente. Solo Admin.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Elimina (soft delete) un vehículo. Solo Admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteVehicleCommand(id), cancellationToken);
        return NoContent();
    }
}
