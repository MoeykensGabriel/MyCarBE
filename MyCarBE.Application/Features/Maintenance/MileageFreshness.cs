namespace MyCarBE.Application.Features.Maintenance;

/// <summary>
/// Qué tan fresca es la última lectura de kilometraje de un vehículo.
///
/// Existe por la estimación de vencimientos: en pantalla, "vence alrededor del 12/10" se ve
/// exactamente igual calculado con una lectura de ayer que con una de hace cuatro meses, y
/// una de las dos fechas no vale nada. Esto es lo que deja calibrar el número.
///
/// El umbral es el mismo del recordatorio del taller (WorkshopSettings.MileageReminderDays) y
/// el criterio espeja a MileageStaleness, que lo aplica sobre el listado de vehículos: sin
/// lectura cuenta como vencido. Si uno cambia, el otro tiene que acompañar.
/// </summary>
public static class MileageFreshness
{
    /// <param name="DaysSince">Días desde la última lectura. Null si nunca hubo.</param>
    /// <param name="IsStale">
    /// Si pasó el umbral del taller. También true cuando nunca hubo lectura: ahí es donde más
    /// falta una.
    /// </param>
    public readonly record struct Freshness(int? DaysSince, bool IsStale);

    public static Freshness Describe(DateTime? lastReadingAt, int reminderDays, DateTime now)
    {
        if (lastReadingAt is not { } lastAt)
            return new Freshness(null, IsStale: true);

        var days = (int)(now - lastAt).TotalDays;
        return new Freshness(days, IsStale: days >= reminderDays);
    }
}
