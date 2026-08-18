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
    private readonly IMaintenanceAlertRepository      _alertRepository;
    private readonly IVehicleBatteryRepository        _batteryRepository;
    private readonly IVehicleTireRepository           _tireRepository;
    private readonly IVehicleMileageReadingRepository _readingRepository;
    private readonly IWorkshopSettingsRepository      _settingsRepository;
    private readonly ICurrentUserService              _currentUser;

    public GetMaintenanceSummaryQueryHandler(
        IMaintenanceAlertRepository      alertRepository,
        IVehicleBatteryRepository        batteryRepository,
        IVehicleTireRepository           tireRepository,
        IVehicleMileageReadingRepository readingRepository,
        IWorkshopSettingsRepository      settingsRepository,
        ICurrentUserService              currentUser)
    {
        _alertRepository    = alertRepository;
        _batteryRepository  = batteryRepository;
        _tireRepository     = tireRepository;
        _readingRepository  = readingRepository;
        _settingsRepository = settingsRepository;
        _currentUser        = currentUser;
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

        // Ritmo de uso por vehículo, para poder decirle al cliente "800 km" Y "unos 16 días".
        // Una sola query para todos los vehículos del cliente: acá una flota puede traer
        // decenas de alertas, y resolver el ritmo de a un vehículo sería un N+1.
        var reminderDays = (await _settingsRepository.GetAsync(cancellationToken)).MileageReminderDays;

        var vehicleIds = alerts.Select(a => a.Vehicle.Id).Distinct().ToList();
        var spans      = await _readingRepository.GetSpansByVehiclesAsync(vehicleIds, cancellationToken);
        var rateByVehicle = spans.ToDictionary(
            kv => kv.Key,
            kv => MileageRateCalculator.Calculate(
                kv.Value.FirstMileage, kv.Value.FirstAt,
                kv.Value.LastMileage,  kv.Value.LastAt,
                kv.Value.ReadingsCount));

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

            rateByVehicle.TryGetValue(alert.Vehicle.Id, out var rate);

            var eval = MaintenanceAlertStatusCalculator.Evaluate(
                alert, alert.Vehicle.CurrentMileage, now, floor, rate);
            if (eval.Severity is null) continue; // todavía no vence → no alerta

            // Si la salud es la que marca la alerta, mostramos ese motivo en vez del contador.
            bool healthDriven = floor is not null && eval.Severity == floor && healthReason is not null;

            var freshness = MileageFreshness.Describe(alert.Vehicle.MileageUpdatedAt, reminderDays, now);

            result.Add(new MaintenanceAlertDto(
                Id:                   alert.Id,
                Type:                 (MaintenanceAlertType)(int)alert.ItemType,
                Severity:             eval.Severity.Value,
                VehicleId:            alert.Vehicle.Id,
                LicensePlate:         alert.Vehicle.LicensePlate,
                Brand:                alert.Vehicle.Brand,
                Model:                alert.Vehicle.Model,
                Title:                alert.Title,
                Detail:               healthDriven ? healthReason! : BuildDetail(eval),
                EstimatedDueDate:     eval.EstimatedDueDate,
                DaysSinceLastReading: freshness.DaysSince,
                ReadingIsStale:       freshness.IsStale));
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
        {
            var km = Common.Formatting.MoneyFormat.ArNumber(Math.Max(0, e.KmRemaining!.Value));

            // Los km solos no le dicen nada al cliente: no sabe si son dos semanas o cinco
            // meses. Si el vehículo tiene ritmo medido, se lo traducimos.
            return e.EstimatedDaysFromKm is { } days
                ? $"Próximo en {km} km — unos {days} días a tu ritmo"
                : $"Próximo en {km} km";
        }

        return $"Próximo en {Math.Max(0, e.DaysRemaining ?? 0)} días";
    }
}
