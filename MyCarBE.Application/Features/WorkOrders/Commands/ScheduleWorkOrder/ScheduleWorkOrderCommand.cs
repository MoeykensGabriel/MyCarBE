using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ScheduleWorkOrder;

/// <summary>
/// Agenda una orden (vehículo) en el calendario de ocupación del taller. Lo hace el Admin.
///
/// Comportamiento:
/// - ScheduledStart = cuándo entra el vehículo / arranca el trabajo.
/// - Si ScheduledEnd viene null, se calcula como Start + duración total estimada de los
///   servicios activos (estimación del mecánico; fallback a la del catálogo).
/// - Si ScheduledStart y ScheduledEnd vienen null, se borra el agendado.
/// - ScheduledEnd debe ser >= ScheduledStart.
/// </summary>
public record ScheduleWorkOrderCommand(
    Guid       WorkOrderId,
    DateTime?  ScheduledStart,
    DateTime?  ScheduledEnd = null
) : IRequest<WorkOrderDetailDto>;
