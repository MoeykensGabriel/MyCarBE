using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.WorkOrderServices.Commands.AcceptService;
using MyCarBE.Application.Features.WorkOrderServices.Commands.AssignMechanic;
using MyCarBE.Application.Features.WorkOrderServices.Commands.ClaimService;
using MyCarBE.Application.Features.WorkOrderServices.Commands.CompleteService;
using MyCarBE.Application.Features.WorkOrderServices.Commands.CompleteServiceAsWorkshop;
using MyCarBE.Application.Features.WorkOrderServices.Commands.ReleaseService;
using MyCarBE.Application.Features.WorkOrderServices.Commands.ScheduleService;
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
    public record ScheduleBody(DateTime? ScheduledStart, DateTime? ScheduledEnd);

    /// <summary>Admin u oficina asigna un mecánico a un servicio.</summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignBody body, CancellationToken cancellationToken)
    {
        await _sender.Send(new AssignMechanicCommand(id, body.MechanicId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Admin u oficina desasigna al mecánico actual del servicio (vuelve al pool).
    /// Vale también para trabajos ya aceptados — destraba servicios cuyo mecánico
    /// no va a continuar (se pelea, renuncia) para que otro los tome.
    /// </summary>
    [HttpPost("{id:guid}/unassign")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new UnassignMechanicCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// El ejecutante se auto-asigna un servicio del pool (Unassigned → Pending).
    /// Requiere que la WO esté en InProgress y que el servicio esté Approved.
    /// Devuelve 409 Conflict si otro lo tomó primero (race condition).
    ///
    /// Admin incluido: si habilitó su perfil de ejecutante trabaja como un mecánico más
    /// (lo toma desde la ficha de la orden). Sin perfil habilitado recibe 403.
    /// </summary>
    [HttpPost("{id:guid}/claim")]
    [Authorize(Roles = "Admin,Mechanic")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Claim(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ClaimServiceCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// El ejecutante libera un servicio que tomó pero todavía no aceptó (Pending → Unassigned).
    /// Vuelve al pool. Solo el dueño actual del Pending puede liberarlo.
    /// </summary>
    [HttpPost("{id:guid}/release")]
    [Authorize(Roles = "Admin,Mechanic")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Release(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ReleaseServiceCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>El ejecutante asignado acepta el trabajo (Pending → Accepted).</summary>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Admin,Mechanic")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new AcceptServiceCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Admin agenda un servicio en el calendario del taller (asigna rango de fechas).
    /// Si ScheduledEnd es null y el servicio tiene EstimatedDurationMinutes, se calcula como
    /// Start + duración (en minutos). Si ambos son null, se borra la programación.
    /// </summary>
    [HttpPost("{id:guid}/schedule")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Schedule(Guid id, [FromBody] ScheduleBody body, CancellationToken cancellationToken)
    {
        await _sender.Send(new ScheduleServiceCommand(id, body.ScheduledStart, body.ScheduledEnd), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// El ejecutante finaliza su propio servicio. Notes obligatorio (mínimo 10 chars).
    /// Es la única vía que persiste Findings — complete-as-workshop solo guarda las notas.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Admin,Mechanic")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteBody body, CancellationToken cancellationToken)
    {
        await _sender.Send(new CompleteServiceCommand(id, body.Notes, body.Findings), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Admin u oficina finaliza en nombre del taller un trabajo de OTRO — para destrabar
    /// servicios cuyo mecánico no va a continuar. Vale tanto para trabajos en curso (Accepted)
    /// como para los que quedaron tomados y nunca arrancaron (Pending): un Pending abandonado
    /// traba la orden entera, que no puede pasar a Completed con servicios sin finalizar.
    /// Notes obligatorio (mínimo 10 chars).
    /// </summary>
    [HttpPost("{id:guid}/complete-as-workshop")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteAsWorkshop(Guid id, [FromBody] CompleteAsWorkshopBody body, CancellationToken cancellationToken)
    {
        await _sender.Send(new CompleteServiceAsWorkshopCommand(id, body.Notes), cancellationToken);
        return NoContent();
    }

    public record CompleteAsWorkshopBody(string Notes);
}
