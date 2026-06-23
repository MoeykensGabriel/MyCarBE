using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Application.Features.VehicleTires;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Maintenance;

/// <summary>
/// Traduce la salud medida de las cubiertas (lo que el técnico midió en la inspección) a una
/// señal para la alerta de mantenimiento. Igual que <see cref="BatteryHealthSignal"/> con la
/// batería: la fila "Cubiertas" deja de ser un mero temporizador y escala si la PEOR cubierta
/// está para cambiar. Healthy/Attention no escalan — la cubierta todavía está para andar.
/// </summary>
public static class TireHealthSignal
{
    /// <summary>
    /// Peor estado entre las cubiertas activas del vehículo (Urgent &gt; ReplaceSoon &gt; …).
    /// Null si no hay cubiertas cargadas. Usa el mismo cálculo que la ficha de cubiertas.
    /// </summary>
    public static TireStatus? WorstStatus(IEnumerable<VehicleTire> tires)
    {
        var statuses = tires.Select(t => TireWearCalculator.Calculate(t).Status).ToList();
        return statuses.Count == 0 ? null : statuses.Max();
    }

    /// <summary>Piso de severidad según el peor estado. Null = no escala.</summary>
    public static MaintenanceAlertSeverity? SeverityFloor(TireStatus status) => status switch
    {
        TireStatus.Urgent      => MaintenanceAlertSeverity.Critical,
        TireStatus.ReplaceSoon => MaintenanceAlertSeverity.Warning,
        _                      => null,
    };

    /// <summary>
    /// Motivo legible para el cliente cuando la salud medida es la que activa la alerta
    /// (en vez del contador de km/tiempo). Null si la salud no escala.
    /// </summary>
    public static string? Reason(TireStatus status) => status switch
    {
        TireStatus.Urgent      => "Según la última revisión del taller, hay cubiertas para cambiar ya.",
        TireStatus.ReplaceSoon => "Según la última revisión del taller, hay cubiertas para cambiar pronto.",
        _                      => null,
    };
}
