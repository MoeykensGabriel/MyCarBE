using MediatR;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Maintenance.Commands.SetVehicleMaintenanceAlerts;

/// <summary>
/// Configura (set "replace") las alertas de mantenimiento de un vehículo. Usado tanto en
/// el ingreso (recepcionista) como en la ficha admin. Reemplaza el set completo:
///   - ítems con Id existente → se actualizan (preservando la línea base),
///   - ítems sin Id → se crean (línea base = km/fecha actuales),
///   - ítems existentes que no vengan en la lista → se borran (soft delete).
/// </summary>
public record SetVehicleMaintenanceAlertsCommand(
    Guid VehicleId,
    IReadOnlyList<MaintenanceAlertItemInput> Items
) : IRequest<IReadOnlyList<MaintenanceAlertConfigDto>>;

/// <summary>Un ítem de la configuración. Al menos uno de IntervalKm/IntervalMonths.</summary>
public record MaintenanceAlertItemInput(
    Guid?               Id,             // null = nuevo
    MaintenanceItemType ItemType,
    string?             Title,          // null/empty = etiqueta por defecto del tipo
    string?             Description,
    int?                IntervalKm,
    int?                IntervalMonths
);
