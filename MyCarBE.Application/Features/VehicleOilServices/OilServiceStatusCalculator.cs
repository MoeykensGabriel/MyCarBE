using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleOilServices;

/// <summary>
/// Regla compartida del estado del aceite: dado el último cambio + el km actual del
/// vehículo, devuelve el estado y los restantes por los dos contadores (km y tiempo).
/// Vive acá (pura, sin EF) para que la use tanto el detalle del vehículo como el
/// resumen de mantenimiento del Inicio — una sola fuente de verdad de los umbrales.
/// </summary>
public static class OilServiceStatusCalculator
{
    // Umbrales de "próximo" — el aviso salta cuando falta poco por km o por tiempo.
    public const int DueSoonKm   = 1000;
    public const int DueSoonDays = 30;

    public readonly record struct OilEvaluation(
        OilServiceStatus Status,
        int              NextServiceKm,
        DateOnly         NextServiceOn,
        int              KmRemaining,
        int              DaysRemaining);

    public static OilEvaluation Evaluate(VehicleOilService oil, int currentMileage)
    {
        var nextServiceKm = oil.ChangedAtKm + oil.IntervalKm;
        var nextServiceOn = oil.ChangedOn.AddMonths(oil.IntervalMonths);

        var today         = DateOnly.FromDateTime(DateTime.UtcNow);
        var kmRemaining   = nextServiceKm - currentMileage;
        var daysRemaining = nextServiceOn.DayNumber - today.DayNumber;

        // El contador que llegue primero manda. Vencido si cualquiera ya se cumplió.
        var status =
            (kmRemaining <= 0 || daysRemaining <= 0)                     ? OilServiceStatus.Overdue
            : (kmRemaining <= DueSoonKm || daysRemaining <= DueSoonDays) ? OilServiceStatus.DueSoon
            : OilServiceStatus.Ok;

        return new OilEvaluation(status, nextServiceKm, nextServiceOn, kmRemaining, daysRemaining);
    }
}
