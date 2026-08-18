using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Maintenance;

/// <summary>
/// Regla compartida del estado de una alerta de mantenimiento configurada: dado el
/// intervalo (km y/o tiempo) y la línea base, devuelve cuánto falta por cada contador
/// y la severidad. Pura (sin EF) — única fuente de verdad de los umbrales, usada por
/// el resumen del Inicio y por la ficha admin. Espeja los umbrales del aceite.
/// </summary>
public static class MaintenanceAlertStatusCalculator
{
    public const int DueSoonKm   = 1000;
    public const int DueSoonDays = 30;

    /// <param name="DaysRemaining">
    /// Días que faltan por el contador de TIEMPO (IntervalMonths). Es una fecha dura: sale
    /// del calendario, no de una estimación.
    /// </param>
    /// <param name="EstimatedDaysFromKm">
    /// Días que faltan por el contador de KM, traducidos con el ritmo de uso del vehículo.
    /// Es una estimación, y por eso viaja aparte de <paramref name="DaysRemaining"/>: meterlas
    /// en el mismo campo sería vender una proyección como si fuera un dato del calendario.
    /// </param>
    /// <param name="EstimatedDueDate">La fecha a la que llega esa estimación.</param>
    public readonly record struct Evaluation(
        MaintenanceAlertSeverity? Severity,   // null = Ok (todavía no alerta)
        int?                      KmRemaining,
        int?                      DaysRemaining,
        int?                      EstimatedDaysFromKm = null,
        DateTime?                 EstimatedDueDate    = null);

    /// <param name="severityFloor">
    /// Piso de severidad opcional aportado por una señal externa al temporizador — hoy, la
    /// salud medida de la batería (lo que el taller vio en la inspección). Solo puede elevar
    /// la severidad por encima de lo que dicen los contadores de km/tiempo, nunca bajarla.
    /// </param>
    /// <param name="rate">
    /// Ritmo de uso del vehículo (km por día), si se pudo calcular. Solo sirve para estimar
    /// una fecha a partir de los km que faltan — NO mueve la severidad. Que una alerta escale
    /// porque "va a" vencer es otra decisión, y conviene tomarla mirando el dato real primero.
    /// Va opcional para no obligar a los llamadores que todavía no lo resuelven.
    /// </param>
    public static Evaluation Evaluate(
        MaintenanceAlert alert, int currentMileage, DateTime now,
        MaintenanceAlertSeverity? severityFloor = null,
        MileageRateCalculator.MileageRate? rate = null)
    {
        int? kmRemaining = alert.IntervalKm.HasValue
            ? (alert.BaselineMileage + alert.IntervalKm.Value) - currentMileage
            : null;

        int? daysRemaining = alert.IntervalMonths.HasValue
            ? (int)Math.Ceiling((alert.BaselineDate.AddMonths(alert.IntervalMonths.Value) - now).TotalDays)
            : null;

        // El contador que llegue primero manda. Vencido si cualquiera ya se cumplió.
        bool overdue = (kmRemaining is <= 0) || (daysRemaining is <= 0);
        bool dueSoon = (kmRemaining is <= DueSoonKm) || (daysRemaining is <= DueSoonDays);

        MaintenanceAlertSeverity? timerSeverity =
            overdue ? MaintenanceAlertSeverity.Critical
            : dueSoon ? MaintenanceAlertSeverity.Warning
            : null;

        // Gana la señal más urgente entre el temporizador y el piso externo.
        MaintenanceAlertSeverity? severity = MostUrgent(timerSeverity, severityFloor);

        // Estimación por km: los km que faltan traducidos a días con el ritmo del vehículo.
        //
        // Solo se estima hacia adelante. Si la alerta ya está vencida por km (kmRemaining <= 0)
        // no hay nada que proyectar: que está vencida ya lo dicen la severidad y los km en
        // negativo, y una fecha en el pasado solo agregaría ruido.
        //
        // Ritmo 0 (auto parado) tampoco estima: a ese ritmo no llega nunca.
        int?      estimatedDaysFromKm = null;
        DateTime? estimatedDueDate    = null;

        if (kmRemaining is { } km && km > 0 && rate is { KmPerDay: > 0 } r)
        {
            estimatedDaysFromKm = (int)Math.Ceiling(km / r.KmPerDay);
            estimatedDueDate    = now.AddDays(estimatedDaysFromKm.Value);
        }

        return new Evaluation(
            severity, kmRemaining, daysRemaining, estimatedDaysFromKm, estimatedDueDate);
    }

    /// <summary>La severidad más urgente de dos (null = sin alerta; Critical &gt; Warning).</summary>
    private static MaintenanceAlertSeverity? MostUrgent(
        MaintenanceAlertSeverity? a, MaintenanceAlertSeverity? b)
    {
        if (a is null) return b;
        if (b is null) return a;
        return (int)a >= (int)b ? a : b;
    }
}
