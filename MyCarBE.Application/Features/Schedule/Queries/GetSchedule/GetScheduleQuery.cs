using MediatR;
using MyCarBE.Application.Features.Schedule.DTOs;

namespace MyCarBE.Application.Features.Schedule.Queries.GetSchedule;

/// <summary>
/// Devuelve los WorkOrderService con scheduling que intersectan el rango [From, To].
/// "Intersectan" = ScheduledStart &lt;= To AND ScheduledEnd &gt;= From.
/// </summary>
public record GetScheduleQuery(DateTime From, DateTime To)
    : IRequest<IReadOnlyList<ScheduleSlotDto>>;
