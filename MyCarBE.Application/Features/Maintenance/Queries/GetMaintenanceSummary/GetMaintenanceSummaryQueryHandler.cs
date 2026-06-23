using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Maintenance.Queries.GetMaintenanceSummary;

/// <summary>
/// Junta las alertas de mantenimiento de todos los vehículos del cliente para el Inicio.
/// Las alertas las configura el recepcionista en el ingreso (intervalo de km y/o tiempo);
/// acá solo se comparan contra el km y la fecha actuales — sin cálculos complejos. El dueño
/// sale del JWT. (Las cards de condición de la ficha —cubiertas/batería/aceite— tienen sus
/// propios endpoints y no dependen de este resumen.)
/// </summary>
public class GetMaintenanceSummaryQueryHandler
    : IRequestHandler<GetMaintenanceSummaryQuery, IReadOnlyList<MaintenanceAlertDto>>
{
    private readonly IMaintenanceAlertRepository _alertRepository;
    private readonly IVehicleBatteryRepository   _batteryRepository;
    private readonly IVehicleTireRepository      _tireRepository;
    private readonly ICurrentUserService         _currentUser;

    public GetMaintenanceSummaryQueryHandler(
        IMaintenanceAlertRepository alertRepository,
        IVehicleBatteryRepository   batteryRepository,
        IVehicleTireRepository      tireRepository,
        ICurrentUserService         currentUser)
    {
        _alertRepository   = alertRepository;
        _batteryRepository = batteryRepository;
        _tireRepository    = tireRepository;
        _currentUser       = currentUser;
    }

    public async Task<IReadOnlyList<MaintenanceAlertDto>> Handle(
        GetMaintenanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var (customerId, fleetId) = ResolveOwner();
        var now    = DateTime.UtcNow;
        var alerts = await _alertRepository.GetActiveByOwnerAsync(customerId, fleetId, cancellationToken);

        // Salud de batería por vehículo (último chequeo), para escalar la fila "Batería" igual
        // que en la ficha: si el taller la marcó para cambiar, aparece acá aunque el
        // temporizador de meses no haya vencido. Una sola query.
        var batteries = await _batteryRepository.GetActiveByOwnerAsync(customerId, fleetId, cancellationToken);
        var batteryStatusByVehicle = batteries
            .GroupBy(b => b.VehicleId)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(b => b.Checks)
                      .OrderByDescending(c => c.CheckedOn)
                      .FirstOrDefault()?.Status);

        // Peor estado de cubiertas por vehículo, mismo criterio que la batería. Una sola query.
        var tires = await _tireRepository.GetActiveTiresByOwnerAsync(customerId, fleetId, cancellationToken);
        var worstTireStatusByVehicle = tires
            .GroupBy(t => t.VehicleId)
            .ToDictionary(g => g.Key, g => TireHealthSignal.WorstStatus(g));

        var result = new List<MaintenanceAlertDto>();
        foreach (var alert in alerts)
        {
            MaintenanceAlertSeverity? floor        = null;
            string?                   healthReason = null;
            if (alert.ItemType == MaintenanceItemType.Battery
                && batteryStatusByVehicle.TryGetValue(alert.Vehicle.Id, out var status)
                && status is BatteryStatus st)
            {
                floor        = BatteryHealthSignal.SeverityFloor(st);
                healthReason = BatteryHealthSignal.Reason(st);
            }
            else if (alert.ItemType == MaintenanceItemType.Tires
                && worstTireStatusByVehicle.TryGetValue(alert.Vehicle.Id, out var tireStatus)
                && tireStatus is TireStatus ts)
            {
                floor        = TireHealthSignal.SeverityFloor(ts);
                healthReason = TireHealthSignal.Reason(ts);
            }

            var eval = MaintenanceAlertStatusCalculator.Evaluate(alert, alert.Vehicle.CurrentMileage, now, floor);
            if (eval.Severity is null) continue; // todavía no vence → no alerta

            // Si la salud es la que marca la alerta, mostramos ese motivo en vez del contador.
            bool healthDriven = floor is not null && eval.Severity == floor && healthReason is not null;

            result.Add(new MaintenanceAlertDto(
                Id:           alert.Id,
                Type:         (MaintenanceAlertType)(int)alert.ItemType,
                Severity:     eval.Severity.Value,
                VehicleId:    alert.Vehicle.Id,
                LicensePlate: alert.Vehicle.LicensePlate,
                Brand:        alert.Vehicle.Brand,
                Model:        alert.Vehicle.Model,
                Title:        alert.Title,
                Detail:       healthDriven ? healthReason! : BuildDetail(eval)));
        }

        // Críticas primero; desempate estable por patente y tipo.
        return result
            .OrderByDescending(a => a.Severity)
            .ThenBy(a => a.LicensePlate)
            .ThenBy(a => a.Type)
            .ToList();
    }

    private (Guid? CustomerId, Guid? FleetId) ResolveOwner()
        => _currentUser.FleetId.HasValue
            ? (null, _currentUser.FleetId)
            : (_currentUser.CustomerId, null);

    private static string BuildDetail(MaintenanceAlertStatusCalculator.Evaluation e)
    {
        if (e.Severity == MaintenanceAlertSeverity.Critical)
            return "Vencido — coordiná el service";

        // Warning: mostramos el contador que esté próximo (km tiene prioridad si aplica).
        if (e.KmRemaining is <= MaintenanceAlertStatusCalculator.DueSoonKm)
            return $"Próximo en {Math.Max(0, e.KmRemaining!.Value):N0} km";

        return $"Próximo en {Math.Max(0, e.DaysRemaining ?? 0)} días";
    }
}
