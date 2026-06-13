namespace MyCarBE.Application.Features.Maintenance.DTOs;

/// <summary>
/// Urgencia de una alerta de mantenimiento, para que el cliente priorice de un
/// vistazo. El front la mapea a color (Warning → ámbar, Critical → rojo).
/// </summary>
public enum MaintenanceAlertSeverity
{
    Warning  = 0,
    Critical = 1,
}
