namespace MyCarBE.Application.Features.Maintenance.DTOs;

/// <summary>
/// Categoría del ítem de mantenimiento de una alerta, para el Inicio del customer.
/// Espeja 1:1 (mismos valores) al enum de dominio <c>MaintenanceItemType</c>.
/// </summary>
public enum MaintenanceAlertType
{
    Oil              = 0,
    Tires            = 1,
    Battery          = 2,
    TimingKit        = 3,
    Transmission     = 4,
    Differential     = 5,
    SparkPlugs       = 6,
    InjectorCleaning = 7,
    Other            = 8,
}
