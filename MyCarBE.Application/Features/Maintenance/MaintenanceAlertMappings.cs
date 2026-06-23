using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Maintenance;

/// <summary>
/// Mapeos y etiquetas compartidas para alertas de mantenimiento configuradas.
/// </summary>
public static class MaintenanceAlertMappings
{
    /// <summary>Etiqueta por defecto según el tipo (si el recepcionista no escribe una).</summary>
    public static string DefaultTitle(MaintenanceItemType type) => type switch
    {
        MaintenanceItemType.Oil              => "Aceite",
        MaintenanceItemType.Tires            => "Cubiertas",
        MaintenanceItemType.Battery          => "Batería",
        MaintenanceItemType.TimingKit        => "Kit de distribución",
        MaintenanceItemType.Transmission     => "Transmisión",
        MaintenanceItemType.Differential     => "Diferenciales",
        MaintenanceItemType.SparkPlugs       => "Bujías",
        MaintenanceItemType.InjectorCleaning => "Limpieza de inyectores",
        _                                    => "Otro",
    };

    /// <summary>Alerta configurada → DTO de configuración (con estado calculado).</summary>
    /// <param name="severityFloor">
    /// Piso de severidad por una señal externa al temporizador (ej. la salud medida de la
    /// batería). Solo eleva la severidad, nunca la baja.
    /// </param>
    /// <param name="healthReason">
    /// Texto a mostrar cuando esa señal externa es la que activa la alerta (en vez del contador).
    /// </param>
    public static MaintenanceAlertConfigDto ToConfigDto(
        this MaintenanceAlert alert, int currentMileage, DateTime now,
        MaintenanceAlertSeverity? severityFloor = null, string? healthReason = null)
    {
        var e = MaintenanceAlertStatusCalculator.Evaluate(alert, currentMileage, now, severityFloor);

        // El motivo de salud solo se muestra si es lo que está marcando (o igualando) la
        // severidad final; si manda el temporizador, dejamos los contadores de siempre.
        string? statusReason =
            severityFloor is not null && e.Severity == severityFloor ? healthReason : null;

        return new MaintenanceAlertConfigDto(
            alert.Id,
            alert.ItemType,
            alert.Title,
            alert.Description,
            alert.IntervalKm,
            alert.IntervalMonths,
            alert.BaselineMileage,
            alert.BaselineDate,
            e.KmRemaining,
            e.DaysRemaining,
            e.Severity,
            statusReason);
    }
}
