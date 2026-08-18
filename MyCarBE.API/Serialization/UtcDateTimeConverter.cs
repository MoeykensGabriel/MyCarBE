using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyCarBE.API.Serialization;

/// <summary>
/// Serializa todo DateTime como UTC explícito (con la "Z" al final).
///
/// El problema que resuelve: las columnas son "timestamp without time zone", así que Npgsql
/// devuelve los DateTime con Kind=Unspecified. System.Text.Json los escribía entonces SIN
/// marca de zona ("2026-08-18T03:51:00"), y el navegador los interpretaba como hora LOCAL.
/// El resultado era que toda la app mostraba 3 horas de más en Argentina — invisible en las
/// fechas sin hora, salvo cerca de medianoche, donde además corría el día entero.
///
/// El valor guardado siempre fue UTC (todo se escribe con DateTime.UtcNow). Lo único que
/// faltaba era decirlo. Por eso Unspecified se trata como UTC y no se convierte nada: no es
/// una traducción de zona, es completar la información que faltaba.
///
/// Local sí se convierte, por si alguna vez entra un DateTime armado con DateTime.Now.
///
/// El arreglo de fondo sería que las columnas fueran timestamptz, pero eso es una migración
/// sobre todas las tablas del sistema y no hace falta para que la hora se vea bien.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc         => value,
            DateTimeKind.Local       => value.ToUniversalTime(),
            _ /* Unspecified */      => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

        writer.WriteStringValue(utc.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"));
    }
}

/// <summary>Igual que <see cref="UtcDateTimeConverter"/> para los nullables.</summary>
public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private static readonly UtcDateTimeConverter Inner = new();

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else               Inner.Write(writer, value.Value, options);
    }
}
