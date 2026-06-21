using MediatR;
using MyCarBE.Application.Features.Maintenance.DTOs;

namespace MyCarBE.Application.Features.Maintenance.Queries.GetVehicleMaintenanceAlerts;

/// <summary>Alertas de mantenimiento configuradas de un vehículo (para la ficha admin).</summary>
public record GetVehicleMaintenanceAlertsQuery(Guid VehicleId)
    : IRequest<IReadOnlyList<MaintenanceAlertConfigDto>>;
