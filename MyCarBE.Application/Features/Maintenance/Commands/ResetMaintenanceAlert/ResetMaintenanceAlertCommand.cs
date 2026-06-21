using MediatR;
using MyCarBE.Application.Features.Maintenance.DTOs;

namespace MyCarBE.Application.Features.Maintenance.Commands.ResetMaintenanceAlert;

/// <summary>
/// Reinicia el ciclo de una alerta (se hizo el service): la línea base pasa a ser el
/// km/fecha actuales, así el próximo vencimiento se cuenta desde ahora.
/// </summary>
public record ResetMaintenanceAlertCommand(Guid VehicleId, Guid AlertId)
    : IRequest<MaintenanceAlertConfigDto>;
