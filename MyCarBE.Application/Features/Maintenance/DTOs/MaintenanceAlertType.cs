namespace MyCarBE.Application.Features.Maintenance.DTOs;

/// <summary>
/// Sistema del vehículo al que apunta una alerta de mantenimiento. Hoy solo
/// cubiertas; aceite y batería se suman después sin cambiar el contrato.
/// </summary>
public enum MaintenanceAlertType
{
    Tire    = 0,
    Oil     = 1,
    Battery = 2,
}
