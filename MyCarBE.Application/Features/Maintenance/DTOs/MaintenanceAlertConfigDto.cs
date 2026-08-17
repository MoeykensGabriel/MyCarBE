using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Maintenance.DTOs;

/// <summary>
/// Configuración de una alerta de mantenimiento de un vehículo (para el panel admin
/// que la crea/edita/reinicia). Incluye el estado calculado (restantes + severidad)
/// como contexto de solo lectura.
/// </summary>
public record MaintenanceAlertConfigDto(
    Guid                      Id,
    MaintenanceItemType       ItemType,
    string                    Title,
    string?                   Description,
    int?                      IntervalKm,
    int?                      IntervalMonths,
    int                       BaselineMileage,
    DateTime                  BaselineDate,
    int?                      KmRemaining,
    int?                      DaysRemaining,
    MaintenanceAlertSeverity? Severity,     // null = todavía no alerta (Ok)
    string?                   StatusReason, // motivo cuando la salud medida manda sobre el contador (ej. batería)

    // ── Estimación por ritmo de uso ───────────────────────────────────────────
    // Los km que faltan traducidos a una fecha. Los tres van juntos o los tres en null:
    // sin lecturas suficientes no hay ritmo y no hay nada que estimar.
    int?                      EstimatedDaysFromKm = null,
    DateTime?                 EstimatedDueDate    = null,

    /// <summary>
    /// El ritmo con el que se hizo la cuenta, en km por día. Viaja a la vista a propósito:
    /// una estimación que no muestra de dónde sale no se puede discutir con el cliente.
    /// </summary>
    decimal?                  KmPerDay            = null,

    // ── Frescura de la lectura ────────────────────────────────────────────────
    // La estimación de arriba vale lo que vale la lectura que tiene atrás, y en pantalla
    // una fecha calculada con datos de ayer se ve igual que una calculada con datos de hace
    // cuatro meses. Esto es lo que permite distinguirlas.
    /// <summary>Días desde la última lectura de kilometraje. Null si nunca hubo.</summary>
    int?                      DaysSinceLastReading = null,
    /// <summary>Pasó el umbral de recordatorio del taller (o nunca hubo lectura).</summary>
    bool                      ReadingIsStale       = false
);
