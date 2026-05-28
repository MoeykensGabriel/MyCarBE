using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.VehicleDocuments.Commands.CreateVehicleDocument;
using MyCarBE.Application.Features.VehicleDocuments.Commands.DeleteVehicleDocument;
using MyCarBE.Application.Features.VehicleDocuments.Commands.UpdateVehicleDocument;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;
using MyCarBE.Application.Features.VehicleDocuments.Queries.GetUpcomingExpirations;
using MyCarBE.Application.Features.VehicleDocuments.Queries.GetVehicleDocuments;
using MyCarBE.Domain.Enums;

namespace MyCarBE.API.Controllers;

/// <summary>
/// CRUD de documentos de un vehículo (VTV, póliza, patente, etc.) y consulta de
/// vencimientos próximos para el cliente actual.
///
/// Ownership en los handlers: Admin todo, Customer solo lo suyo, Fleet Contact su flota.
/// </summary>
[ApiController]
[Authorize]
public class VehicleDocumentsController : ControllerBase
{
    private readonly ISender _sender;
    public VehicleDocumentsController(ISender sender) => _sender = sender;

    // Body records (sin VehicleId/Id — vienen por ruta)
    public record CreateBody(VehicleDocumentType DocumentType, DateOnly ExpiresOn, string? Notes, string? IssuingEntity);
    public record UpdateBody(VehicleDocumentType DocumentType, DateOnly ExpiresOn, string? Notes, string? IssuingEntity);

    /// <summary>Lista los documentos de un vehículo.</summary>
    [HttpGet("api/vehicles/{vehicleId:guid}/documents")]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid vehicleId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetVehicleDocumentsQuery(vehicleId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Agrega un documento al vehículo.</summary>
    [HttpPost("api/vehicles/{vehicleId:guid}/documents")]
    [ProducesResponseType(typeof(VehicleDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid vehicleId, [FromBody] CreateBody body, CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(new CreateVehicleDocumentCommand(
            vehicleId, body.DocumentType, body.ExpiresOn, body.Notes, body.IssuingEntity),
            cancellationToken);
        return CreatedAtAction(nameof(List), new { vehicleId }, dto);
    }

    /// <summary>Actualiza un documento.</summary>
    [HttpPatch("api/vehicles/{vehicleId:guid}/documents/{id:guid}")]
    [ProducesResponseType(typeof(VehicleDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid vehicleId, Guid id, [FromBody] UpdateBody body, CancellationToken cancellationToken)
    {
        _ = vehicleId; // se valida en el handler por VehicleId del doc
        var dto = await _sender.Send(new UpdateVehicleDocumentCommand(
            id, body.DocumentType, body.ExpiresOn, body.Notes, body.IssuingEntity),
            cancellationToken);
        return Ok(dto);
    }

    /// <summary>Elimina (soft delete) un documento.</summary>
    [HttpDelete("api/vehicles/{vehicleId:guid}/documents/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid vehicleId, Guid id, CancellationToken cancellationToken)
    {
        _ = vehicleId;
        await _sender.Send(new DeleteVehicleDocumentCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Vencimientos próximos del usuario actual (Customer o Fleet Contact).
    /// horizon = días hacia adelante (default 60).
    /// </summary>
    [HttpGet("api/customers/me/upcoming-expirations")]
    [ProducesResponseType(typeof(IReadOnlyList<UpcomingExpirationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpcomingForMe([FromQuery] int horizon = 60, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetUpcomingExpirationsQuery(horizon), cancellationToken);
        return Ok(result);
    }
}
