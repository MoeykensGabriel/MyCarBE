using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.WorkOrderServices.Commands.AcceptService;
using MyCarBE.Application.Features.WorkOrderServices.Commands.AssignMechanic;
using MyCarBE.Application.Features.WorkOrderServices.Commands.CompleteService;
using MyCarBE.Application.Features.WorkOrderServices.Commands.UnassignMechanic;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Endpoints centrados en el ciclo de vida de un WorkOrderService individual.
/// La gestión global de la WorkOrder está en WorkOrdersController.
/// </summary>
[ApiController]
[Route("api/work-order-services")]
[Authorize]
public class WorkOrderServicesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkOrderServicesController(ISender sender) => _sender = sender;

    public record AssignBody(Guid MechanicId);
    public record CompleteBody(string Notes, string? Findings);

    /// <summary>Admin asigna un mecánico a un servicio.</summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignBody body, CancellationToken cancellationToken)
    {
        await _sender.Send(new AssignMechanicCommand(id, body.MechanicId), cancellationToken);
        return NoContent();
    }

    /// <summary>Admin desasigna al mecánico actual del servicio.</summary>
    [HttpPost("{id:guid}/unassign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new UnassignMechanicCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>El mecánico asignado acepta el trabajo (Pending → Accepted).</summary>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Mechanic")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new AcceptServiceCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>El mecánico finaliza el servicio. Notes obligatorio (mínimo 10 chars).</summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Mechanic")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteBody body, CancellationToken cancellationToken)
    {
        await _sender.Send(new CompleteServiceCommand(id, body.Notes, body.Findings), cancellationToken);
        return NoContent();
    }
}
