using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Application.Features.VehicleDocuments; // VehicleOwnershipGuard

namespace MyCarBE.Application.Features.Maintenance.Queries.GetVehicleMaintenanceAlerts;

public class GetVehicleMaintenanceAlertsQueryHandler
    : IRequestHandler<GetVehicleMaintenanceAlertsQuery, IReadOnlyList<MaintenanceAlertConfigDto>>
{
    private readonly IVehicleRepository          _vehicleRepository;
    private readonly IMaintenanceAlertRepository _alertRepository;
    private readonly ICurrentUserService         _currentUser;

    public GetVehicleMaintenanceAlertsQueryHandler(
        IVehicleRepository          vehicleRepository,
        IMaintenanceAlertRepository alertRepository,
        ICurrentUserService         currentUser)
    {
        _vehicleRepository = vehicleRepository;
        _alertRepository   = alertRepository;
        _currentUser       = currentUser;
    }

    public async Task<IReadOnlyList<MaintenanceAlertConfigDto>> Handle(
        GetVehicleMaintenanceAlertsQuery request, CancellationToken cancellationToken)
    {
        // Admin / dueño del vehículo / contacto de la flota pueden verlas. Otro rol → 404.
        // Así el cliente ve, en la ficha de su vehículo, las alertas que le configuró el taller.
        var vehicle = await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        var now    = DateTime.UtcNow;
        var alerts = await _alertRepository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);

        return alerts.Select(a => a.ToConfigDto(vehicle.CurrentMileage, now)).ToList();
    }
}
