using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Maintenance;

/// <summary>
/// Traduce la salud medida de la batería (la que carga el mecánico en la inspección) a una
/// señal para la alerta de mantenimiento. Así la fila "Batería" deja de ser un simple
/// temporizador de meses y respeta lo que el taller efectivamente vio: si la última revisión
/// dice "cambiar pronto/ya", la alerta escala aunque el intervalo de tiempo no haya vencido.
/// Good/Fair no escalan — la batería todavía está para andar.
/// </summary>
public static class BatteryHealthSignal
{
    /// <summary>Piso de severidad según el último chequeo. Null = no escala.</summary>
    public static MaintenanceAlertSeverity? SeverityFloor(BatteryStatus status) => status switch
    {
        BatteryStatus.Replace     => MaintenanceAlertSeverity.Critical,
        BatteryStatus.ReplaceSoon => MaintenanceAlertSeverity.Warning,
        _                         => null,
    };

    /// <summary>
    /// Motivo legible para el cliente cuando la salud medida es la que activa la alerta
    /// (en vez del contador de km/tiempo). Null si la salud no escala.
    /// </summary>
    public static string? Reason(BatteryStatus status) => status switch
    {
        BatteryStatus.Replace     => "Según la última revisión del taller, conviene reemplazarla.",
        BatteryStatus.ReplaceSoon => "Según la última revisión del taller, conviene cambiarla pronto.",
        _                         => null,
    };
}
