using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.Schedule.DTOs;
using MyCarBE.Application.Features.Schedule.Queries.GetSchedule;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Calendario de turnos del taller. Devuelve los servicios agendados que
/// intersectan un rango de fechas, para que el FE arme la grilla día × área.
/// </summary>
[ApiController]
[Route("api/schedule")]
[Authorize(Roles = "Admin,Receptionist")]
public class ScheduleController : ControllerBase
{
    private readonly ISender _sender;
    public ScheduleController(ISender sender) => _sender = sender;

    /// <summary>
    /// Devuelve los servicios agendados que intersectan [from, to] (ambos inclusive).
    /// Si no se pasan parámetros, usa la semana actual (lunes a domingo).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ScheduleSlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSchedule(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var (fromDate, toDate) = ResolveRange(from, to);

        if (toDate < fromDate)
            return BadRequest("'to' debe ser >= 'from'.");

        if ((toDate - fromDate).TotalDays > 92)
            return BadRequest("El rango no puede exceder 92 días.");

        var result = await _sender.Send(new GetScheduleQuery(fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    private static (DateTime From, DateTime To) ResolveRange(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue)
            return (from.Value.Date, to.Value.Date);

        // Default: semana actual (lunes a domingo)
        var today = DateTime.UtcNow.Date;
        var diff  = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var monday = today.AddDays(-diff);
        return (monday, monday.AddDays(6));
    }
}
