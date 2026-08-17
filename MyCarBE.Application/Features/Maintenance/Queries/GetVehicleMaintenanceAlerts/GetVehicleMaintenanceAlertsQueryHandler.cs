using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Application.Features.VehicleDocuments; // VehicleOwnershipGuard
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Maintenance.Queries.GetVehicleMaintenanceAlerts;

public class GetVehicleMaintenanceAlertsQueryHandler
    : IRequestHandler<GetVehicleMaintenanceAlertsQuery, IReadOnlyList<MaintenanceAlertConfigDto>>
{
    private readonly IVehicleRepository               _vehicleRepository;
    private readonly IMaintenanceAlertRepository      _alertRepository;
    private readonly IVehicleBatteryRepository        _batteryRepository;
    private readonly IVehicleTireRepository           _tireRepository;
    private readonly IVehicleMileageReadingRepository _readingRepository;
    private readonly ICurrentUserService              _currentUser;

    public GetVehicleMaintenanceAlertsQueryHandler(
        IVehicleRepository               vehicleRepository,
        IMaintenanceAlertRepository      alertRepository,
        IVehicleBatteryRepository        batteryRepository,
        IVehicleTireRepository           tireRepository,
        IVehicleMileageReadingRepository readingRepository,
        ICurrentUserService              currentUser)
    {
        _vehicleRepository = vehicleRepository;
        _alertRepository   = alertRepository;
        _batteryRepository = batteryRepository;
        _tireRepository    = tireRepository;
        _readingRepository = readingRepository;
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

        // Ritmo de uso, para estimar cuándo va a vencer cada alerta. Va por los extremos del
        // historial y no por las últimas N lecturas: el ritmo se mide DESDE la primera, así que
        // acotar el histórico daría un punto de partida equivocado apenas el vehículo acumule
        // lecturas. Si todavía no hay dos separadas por un día, queda en null y no se estima.
        var spans = await _readingRepository.GetSpansByVehiclesAsync(
            new[] { request.VehicleId }, cancellationToken);

        MileageRateCalculator.MileageRate? rate = spans.TryGetValue(request.VehicleId, out var span)
            ? MileageRateCalculator.Calculate(
                span.FirstMileage, span.FirstAt, span.LastMileage, span.LastAt, span.ReadingsCount)
            : null;

        // Si el taller configuró alerta de batería o de cubiertas, traemos la salud medida para
        // que esas filas respeten lo que vio en la inspección y no sean meros temporizadores.
        BatteryStatus? batteryStatus = null;
        if (alerts.Any(a => a.ItemType == MaintenanceItemType.Battery))
        {
            var battery = (await _batteryRepository
                .GetByVehicleAsync(request.VehicleId, cancellationToken: cancellationToken))
                .FirstOrDefault();
            batteryStatus = battery?.Checks
                .OrderByDescending(c => c.CheckedOn)
                .FirstOrDefault()?.Status;
        }

        TireStatus? worstTireStatus = null;
        if (alerts.Any(a => a.ItemType == MaintenanceItemType.Tires))
        {
            var tires = await _tireRepository
                .GetByVehicleAsync(request.VehicleId, cancellationToken: cancellationToken);
            worstTireStatus = TireHealthSignal.WorstStatus(tires);
        }

        return alerts.Select(a =>
        {
            MaintenanceAlertSeverity? floor  = null;
            string?                   reason = null;
            if (a.ItemType == MaintenanceItemType.Battery && batteryStatus is BatteryStatus bs)
            {
                floor  = BatteryHealthSignal.SeverityFloor(bs);
                reason = BatteryHealthSignal.Reason(bs);
            }
            else if (a.ItemType == MaintenanceItemType.Tires && worstTireStatus is TireStatus ts)
            {
                floor  = TireHealthSignal.SeverityFloor(ts);
                reason = TireHealthSignal.Reason(ts);
            }
            return a.ToConfigDto(vehicle.CurrentMileage, now, floor, reason, rate);
        }).ToList();
    }
}
