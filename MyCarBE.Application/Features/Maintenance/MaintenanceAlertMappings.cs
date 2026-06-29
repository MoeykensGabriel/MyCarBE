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

    /// <summary>
    /// ítems cuyo intervalo se cuenta "desde fábrica" (desde 0 km), no desde el último
    /// service: transmisión, distribución, diferenciales, bujías. Para estos, al crear la
    /// alerta sin saber el último cambio, la línea base se alinea al múltiplo de fábrica
    /// más cercano por abajo en vez del km de ingreso — así avisa en el próximo hito real
    /// (ej. auto que entra a 40.000 con intervalo 60.000 → avisa a 60.000, no a 100.000).
    /// El resto (aceite, inyectores, cubiertas, batería) arranca el ciclo en el ingreso.
    /// </summary>
    public static bool IsFactoryMilestone(MaintenanceItemType type) => type switch
    {
        MaintenanceItemType.TimingKit    => true,
        MaintenanceItemType.Transmission => true,
        MaintenanceItemType.Differential => true,
        MaintenanceItemType.SparkPlugs   => true,
        _                                => false,
    };

    /// <summary>
    /// Línea base de km para una alerta NUEVA, según lo que se sepa, en orden de prioridad:
    ///   1. el km del último cambio si el recepcionista lo cargó (lastServiceMileage);
    ///   2. el múltiplo de fábrica más cercano por abajo del km actual, si el ítem se mide
    ///      desde fábrica y tiene intervalo de km (el "factor de corrección");
    ///   3. el km actual (ciclo que arranca en el ingreso, como el aceite).
    /// La división entera ya hace el floor para no negativos, así que no hace falta más.
    /// </summary>
    public static int ResolveBaselineMileage(
        MaintenanceItemType type, int? intervalKm, int currentMileage, int? lastServiceMileage)
    {
        if (lastServiceMileage is { } last)
            return last;

        if (IsFactoryMilestone(type) && intervalKm is { } interval && interval > 0)
            return (currentMileage / interval) * interval;

        return currentMileage;
    }

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
