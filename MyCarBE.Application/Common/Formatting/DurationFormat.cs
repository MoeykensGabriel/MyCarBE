namespace MyCarBE.Application.Common.Formatting;

/// <summary>
/// Duraciones legibles: días → horas → minutos, salteando lo que da cero.
/// "150 min" no se lo imagina nadie; "2 h 30 min" sí.
///
/// Espeja formatDuration() del frontend a propósito: la misma duración tiene que leerse
/// igual en pantalla y en el PDF, o el cliente cree que son dos datos distintos.
/// </summary>
public static class DurationFormat
{
    /// <summary>Minutos por jornada laboral. 1 día = 8 hs = 480 min.</summary>
    public const int MinutesPerWorkday = 480;

    /// <summary>
    /// "45 min", "2 h", "2 h 30 min", "1 día 1 h", "2 días 40 min".
    /// Devuelve null para 0 o negativos, para que el caller no dibuje el campo.
    /// </summary>
    public static string? ArDuration(int minutes)
    {
        if (minutes <= 0) return null;

        var days   = minutes / MinutesPerWorkday;
        var hours  = minutes % MinutesPerWorkday / 60;
        var remMin = minutes % 60;

        var parts = new List<string>(3);
        if (days > 0)   parts.Add(days == 1 ? "1 día" : $"{days} días");
        if (hours > 0)  parts.Add($"{hours} h");
        if (remMin > 0) parts.Add($"{remMin} min");

        return string.Join(" ", parts);
    }
}
