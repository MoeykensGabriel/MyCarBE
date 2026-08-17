namespace MyCarBE.Application.Features.Maintenance.DTOs;

/// <summary>
/// Una alerta de mantenimiento de un vehículo del cliente, para el tablero de Inicio.
/// El resumen junta las de todos sus vehículos en una sola respuesta.
///   - Title:  etiqueta corta del sistema afectado, ej. "Cubiertas".
///   - Detail: frase accionable, ej. "2 cubiertas en estado crítico — cambio inmediato".
/// </summary>
public record MaintenanceAlertDto(
    Guid                     Id,
    MaintenanceAlertType     Type,
    MaintenanceAlertSeverity Severity,
    Guid                     VehicleId,
    string                   LicensePlate,
    string                   Brand,
    string                   Model,
    string                   Title,
    string                   Detail,

    /// <summary>
    /// Cuándo se estima que vence, según el ritmo de uso del vehículo. Null cuando no hay
    /// lecturas suficientes para medirlo, cuando el auto está parado, o cuando ya venció
    /// (ahí no hay nada que proyectar). El texto de <paramref name="Detail"/> ya lo
    /// incorpora; el campo va aparte para que la card pueda mostrarlo como fecha.
    /// </summary>
    DateTime?                EstimatedDueDate = null,

    /// <summary>Días desde la última lectura de kilometraje del vehículo. Null si nunca hubo.</summary>
    int?                     DaysSinceLastReading = null,

    /// <summary>
    /// La lectura pasó el umbral de recordatorio del taller (o nunca hubo). Cuando es true, la
    /// estimación de arriba se apoya en un dato viejo y conviene decirlo en vez de mostrar la
    /// fecha con la misma seguridad que una calculada con datos frescos.
    /// </summary>
    bool                     ReadingIsStale = false
);
