using MediatR;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.ScheduleService;

/// <summary>
/// Agenda un WorkOrderService en el calendario del taller. Lo hace el Admin.
///
/// Comportamiento:
/// - Si ScheduledEnd es null y EstimatedDurationMinutes está poblado en el servicio, se calcula como Start + duración.
/// - Si ambos vienen null, se borra la programación.
/// - ScheduledEnd debe ser >= ScheduledStart.
/// </summary>
public record ScheduleServiceCommand(
    Guid       WorkOrderServiceId,
    DateTime?  ScheduledStart,
    DateTime?  ScheduledEnd
) : IRequest;
