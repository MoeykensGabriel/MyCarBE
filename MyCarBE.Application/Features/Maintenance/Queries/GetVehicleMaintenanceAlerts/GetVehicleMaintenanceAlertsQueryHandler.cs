using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Maintenance.Queries.GetVehicleMaintenanceAlerts;

public class GetVehicleMaintenanceAlertsQueryHandler
    : IRequestHandler<GetVehicleMaintenanceAlertsQuery, IReadOnlyList<MaintenanceAlertConfigDto>>
{
    private readonly IVehicleRepository          _vehicleRepository;
    private readonly IMaintenanceAlertRepository _alertRepository;

    public GetVehicleMaintenanceAlertsQueryHandler(
        IVehicleRepository          vehicleRepository,
        IMaintenanceAlertRepository alertRepository)
    {
        _vehicleRepository = vehicleRepository;
        _alertRepository   = alertRepository;
    }

    public async Task<IReadOnlyList<MaintenanceAlertConfigDto>> Handle(
        GetVehicleMaintenanceAlertsQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        var now    = DateTime.UtcNow;
        var alerts = await _alertRepository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);

        return alerts.Select(a => a.ToConfigDto(vehicle.CurrentMileage, now)).ToList();
    }
}
