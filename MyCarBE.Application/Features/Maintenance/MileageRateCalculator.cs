using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Maintenance;

/// <summary>
/// Ritmo de uso del vehículo: cuántos km por día hace, según las lecturas reales del
/// odómetro que ya se vienen guardando (la del ingreso al taller y las que carga el
/// cliente). Es lo que después permite traducir "te faltan 800 km" a una fecha.
///
/// Se mide entre la PRIMERA y la ÚLTIMA lectura, no sobre una ventana móvil de las
/// últimas: la primera lectura es el punto de partida real del auto en el sistema y
/// el promedio largo no se sacude por un viaje puntual. Si algún día se quiere que
/// reaccione más rápido a un cambio de uso, se acota acá y no toca a nadie más.
///
/// Pura (sin EF / sin DB) para poder testearla aislada, igual que
/// <see cref="MaintenanceAlertStatusCalculator"/> y TireWearCalculator.
/// </summary>
public static class MileageRateCalculator
{
    /// <summary>
    /// Días mínimos que tienen que separar la primera lectura de la última. Con menos de
    /// un día el cociente no significa nada: dos lecturas de la misma mañana darían un
    /// ritmo enorme o una división por cero. El caso típico es el ingreso al taller, que
    /// registra su lectura minutos después de que el cliente cargó la suya.
    /// </summary>
    public const int MinDaysSpanned = 1;

    /// <param name="KmPerDay">
    /// Puede ser 0: el auto está parado. Eso NO es lo mismo que no saber el ritmo — se
    /// devuelve el 0 y quien estime decide qué hacer (no se puede proyectar una fecha,
    /// porque a ritmo 0 no llega nunca).
    /// </param>
    /// <param name="DaysSpanned">Días entre la primera y la última lectura.</param>
    /// <param name="KmSpanned">Km recorridos en esos días.</param>
    /// <param name="ReadingsUsed">
    /// Cuántas lecturas hay detrás. No entra en la cuenta, pero sí en la confianza: un
    /// ritmo sacado de 2 lecturas de una semana no se muestra igual que uno de 12 lecturas
    /// a lo largo de un año.
    /// </param>
    public readonly record struct MileageRate(
        decimal KmPerDay,
        int     DaysSpanned,
        int     KmSpanned,
        int     ReadingsUsed);

    /// <summary>
    /// Ritmo a partir del historial completo de lecturas del vehículo. Las ordena por fecha
    /// y usa los extremos. Devuelve null si no alcanza para calcular nada.
    /// </summary>
    public static MileageRate? Calculate(IReadOnlyList<VehicleMileageReading>? readings)
    {
        if (readings is null || readings.Count < 2) return null;

        var ordered = readings.OrderBy(r => r.CreatedAt).ToList();
        var first   = ordered[0];
        var last    = ordered[^1];

        return Calculate(
            first.Mileage, first.CreatedAt,
            last.Mileage,  last.CreatedAt,
            ordered.Count);
    }

    /// <summary>
    /// Ritmo a partir de los dos extremos ya resueltos. Es la versión que usa el camino del
    /// Inicio del cliente, donde la consulta trae directamente la primera y la última lectura
    /// de cada vehículo y sería un desperdicio traer el historial entero.
    /// </summary>
    public static MileageRate? Calculate(
        int firstMileage, DateTime firstAt,
        int lastMileage,  DateTime lastAt,
        int readingsUsed)
    {
        var daysSpanned = (int)Math.Floor((lastAt - firstAt).TotalDays);
        if (daysSpanned < MinDaysSpanned) return null;

        var kmSpanned = lastMileage - firstMileage;

        // El odómetro no retrocede: ReportVehicleMileageCommandHandler lo impide. Si igual
        // llegara un negativo (dato viejo, corrección manual), no proyectamos sobre eso:
        // más vale no decir nada que decir una fecha inventada.
        if (kmSpanned < 0) return null;

        return new MileageRate(
            KmPerDay:     decimal.Round((decimal)kmSpanned / daysSpanned, 2),
            DaysSpanned:  daysSpanned,
            KmSpanned:    kmSpanned,
            ReadingsUsed: readingsUsed);
    }
}
