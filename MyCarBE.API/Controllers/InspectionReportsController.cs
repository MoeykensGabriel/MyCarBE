using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.InspectionReports.Commands.CreateInspectionReport;
using MyCarBE.Application.Features.InspectionReports.Commands.UpdateInspectionReport;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Application.Features.InspectionReports.Queries.GetInspectionReportById;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/inspection-reports")]
[Authorize]
public class InspectionReportsController : ControllerBase
{
    private readonly ISender _sender;

    public InspectionReportsController(ISender sender) => _sender = sender;

    /// <summary>
    /// El mecánico crea un reporte sobre un área de una orden.
    /// Validaciones: debe estar asignado al área; solo UN reporte por (orden, área).
    ///
    /// La oficina (admin o recepción) también puede reportar — inspecciona ella misma
    /// cuando no hay mecánico del área disponible. Entra como generalista (cualquier área
    /// activa) y el reporte queda con MechanicId null, o sea firmado por la oficina.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    [ProducesResponseType(typeof(InspectionReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateInspectionReportCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// El mecánico que creó el reporte lo puede editar (Findings, HasIssue).
    /// No se puede editar un reporte marcado como "sin hallazgos" por el admin.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Mechanic")]
    [ProducesResponseType(typeof(InspectionReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInspectionReportCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id does not match body id.");
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// GET de un reporte individual. Necesario para el CreatedAtAction y para
    /// eventuales refresh del FE. Devuelve 200 si el caller tiene visibilidad.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InspectionReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        // Reuso del repo via MediatR sería overkill — devolvemos directo desde el query del listado
        // de la orden contenedora. Pero como exponemos /{id} para el CreatedAtAction, hacemos un
        // pequeño bypass: enviamos un GetById dedicado a través del listado filtrado.
        // (Si en el futuro necesita auth fina por rol, se mueve a un query propio.)
        var dto = await _sender.Send(new GetInspectionReportByIdQuery(id), cancellationToken);
        return Ok(dto);
    }
}

