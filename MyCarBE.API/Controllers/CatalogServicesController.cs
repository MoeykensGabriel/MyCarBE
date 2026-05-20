using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Features.CatalogServices.Commands.CreateCatalogService;
using MyCarBE.Application.Features.CatalogServices.Commands.DeleteCatalogService;
using MyCarBE.Application.Features.CatalogServices.Commands.UpdateCatalogService;
using MyCarBE.Application.Features.CatalogServices.DTOs;
using MyCarBE.Application.Features.CatalogServices.Queries.GetAllCatalogServices;
using MyCarBE.Application.Features.CatalogServices.Queries.GetCatalogServiceById;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/catalog-services")]
[Authorize]
public class CatalogServicesController : ControllerBase
{
    private readonly ISender             _sender;
    private readonly ICurrentUserService _currentUser;

    public CatalogServicesController(ISender sender, ICurrentUserService currentUser)
    {
        _sender      = sender;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Retorna todos los servicios del catálogo.
    /// Admin puede incluir inactivos con ?includeInactive=true.
    /// Customer solo ve los activos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        // Solo Admin puede solicitar incluir inactivos
        var query  = new GetAllCatalogServicesQuery(_currentUser.IsAdmin && includeInactive);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna un servicio por Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CatalogServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCatalogServiceByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo servicio en el catálogo. Solo Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCatalogServiceCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Actualiza un servicio existente. Solo Admin.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CatalogServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCatalogServiceCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Elimina (soft delete) un servicio del catálogo. Solo Admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCatalogServiceCommand(id), cancellationToken);
        return NoContent();
    }
}
