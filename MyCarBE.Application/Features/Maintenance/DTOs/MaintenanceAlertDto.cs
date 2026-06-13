namespace MyCarBE.Application.Features.Maintenance.DTOs;

/// <summary>
/// Una alerta de mantenimiento de un vehículo del cliente, para el tablero de Inicio.
/// El resumen junta las de todos sus vehículos en una sola respuesta.
///   - Title:  etiqueta corta del sistema afectado, ej. "Cubiertas".
///   - Detail: frase accionable, ej. "2 cubiertas en estado crítico — cambio inmediato".
/// </summary>
public record MaintenanceAlertDto(
    MaintenanceAlertType     Type,
    MaintenanceAlertSeverity Severity,
    Guid                     VehicleId,
    string                   LicensePlate,
    string                   Brand,
    string                   Model,
    string                   Title,
    string                   Detail
);
