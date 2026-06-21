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
    MaintenanceAlertSeverity? Severity      // null = todavía no alerta (Ok)
);
