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

    public readonly record struct Evaluation(
        MaintenanceAlertSeverity? Severity,   // null = Ok (todavía no alerta)
        int?                      KmRemaining,
        int?                      DaysRemaining);

    public static Evaluation Evaluate(MaintenanceAlert alert, int currentMileage, DateTime now)
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

        MaintenanceAlertSeverity? severity =
            overdue ? MaintenanceAlertSeverity.Critical
            : dueSoon ? MaintenanceAlertSeverity.Warning
            : null;

        return new Evaluation(severity, kmRemaining, daysRemaining);
    }
}
