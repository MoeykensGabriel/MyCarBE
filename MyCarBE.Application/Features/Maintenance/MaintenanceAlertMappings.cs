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
    public static MaintenanceAlertConfigDto ToConfigDto(
        this MaintenanceAlert alert, int currentMileage, DateTime now)
    {
        var e = MaintenanceAlertStatusCalculator.Evaluate(alert, currentMileage, now);
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
            e.Severity);
    }
}
