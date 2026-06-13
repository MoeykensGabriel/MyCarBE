using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Application.Features.VehicleOilServices; // OilServiceStatusCalculator
using MyCarBE.Application.Features.VehicleTires;       // TireWearCalculator
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Maintenance.Queries.GetMaintenanceSummary;

/// <summary>
/// Junta las alertas de mantenimiento de todos los vehículos del cliente: cubiertas,
/// aceite y batería. Cada sistema reutiliza su propia regla (la misma que la card del
/// detalle) y aporta a lo sumo una alerta por vehículo. El dueño sale del JWT.
/// </summary>
public class GetMaintenanceSummaryQueryHandler
    : IRequestHandler<GetMaintenanceSummaryQuery, IReadOnlyList<MaintenanceAlertDto>>
{
    private readonly IVehicleTireRepository        _tireRepository;
    private readonly IVehicleOilServiceRepository  _oilRepository;
    private readonly IVehicleBatteryRepository     _batteryRepository;
    private readonly ICurrentUserService           _currentUser;

    public GetMaintenanceSummaryQueryHandler(
        IVehicleTireRepository       tireRepository,
        IVehicleOilServiceRepository oilRepository,
        IVehicleBatteryRepository    batteryRepository,
        ICurrentUserService          currentUser)
    {
        _tireRepository    = tireRepository;
        _oilRepository     = oilRepository;
        _batteryRepository = batteryRepository;
        _currentUser       = currentUser;
    }

    public async Task<IReadOnlyList<MaintenanceAlertDto>> Handle(
        GetMaintenanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var (customerId, fleetId) = ResolveOwner();

        var tires      = await _tireRepository.GetActiveTiresByOwnerAsync(customerId, fleetId, cancellationToken);
        var oils       = await _oilRepository.GetLatestByOwnerAsync(customerId, fleetId, cancellationToken);
        var batteries  = await _batteryRepository.GetActiveByOwnerAsync(customerId, fleetId, cancellationToken);

        var alerts = new List<MaintenanceAlertDto>();
        alerts.AddRange(BuildTireAlerts(tires));
        alerts.AddRange(BuildOilAlerts(oils));
        alerts.AddRange(BuildBatteryAlerts(batteries));

        // Críticas primero; desempate estable por patente y sistema.
        return alerts
            .OrderByDescending(a => a.Severity)
            .ThenBy(a => a.LicensePlate)
            .ThenBy(a => a.Type)
            .ToList();
    }

    private (Guid? CustomerId, Guid? FleetId) ResolveOwner()
        => _currentUser.FleetId.HasValue
            ? (null, _currentUser.FleetId)
            : (_currentUser.CustomerId, null);

    // ─── Cubiertas ───────────────────────────────────────────────────────────────
    // Una alerta por vehículo: agrupa sus cubiertas y resume el peor estado.
    // Attention y Healthy no alertan (no saturar el Inicio).

    private static IEnumerable<MaintenanceAlertDto> BuildTireAlerts(IReadOnlyList<VehicleTire> tires)
    {
        foreach (var group in tires.GroupBy(t => t.VehicleId))
        {
            var vehicle = group.First().Vehicle;
            int urgent = 0, replaceSoon = 0, irregular = 0;

            foreach (var tire in group)
            {
                var estimation = TireWearCalculator.Calculate(tire);
                if      (estimation.Status == TireStatus.Urgent)      urgent++;
                else if (estimation.Status == TireStatus.ReplaceSoon) replaceSoon++;
                if (estimation.HasIrregularWear) irregular++;
            }

            if (urgent == 0 && replaceSoon == 0 && irregular == 0) continue;

            yield return Alert(
                MaintenanceAlertType.Tire,
                urgent > 0 ? MaintenanceAlertSeverity.Critical : MaintenanceAlertSeverity.Warning,
                vehicle,
                "Cubiertas",
                BuildTireDetail(urgent, replaceSoon, irregular));
        }
    }

    private static string BuildTireDetail(int urgent, int replaceSoon, int irregular)
    {
        if (urgent > 0)
            return urgent == 1
                ? "1 cubierta en estado crítico — cambio inmediato"
                : $"{urgent} cubiertas en estado crítico — cambio inmediato";

        if (replaceSoon > 0)
            return replaceSoon == 1
                ? "1 cubierta para cambiar pronto"
                : $"{replaceSoon} cubiertas para cambiar pronto";

        return irregular == 1
            ? "Desgaste irregular en 1 cubierta — conviene revisión"
            : $"Desgaste irregular en {irregular} cubiertas — conviene revisión";
    }

    // ─── Aceite ──────────────────────────────────────────────────────────────────
    // Overdue → crítico, DueSoon → atención. Ok no alerta.

    private static IEnumerable<MaintenanceAlertDto> BuildOilAlerts(IReadOnlyList<VehicleOilService> oils)
    {
        foreach (var oil in oils)
        {
            var eval = OilServiceStatusCalculator.Evaluate(oil, oil.Vehicle.CurrentMileage);
            if (eval.Status == OilServiceStatus.Ok) continue;

            var (severity, detail) = eval.Status == OilServiceStatus.Overdue
                ? (MaintenanceAlertSeverity.Critical, "Cambio de aceite vencido")
                : (MaintenanceAlertSeverity.Warning,
                   eval.KmRemaining <= OilServiceStatusCalculator.DueSoonKm
                       ? $"Próximo cambio de aceite en {eval.KmRemaining:N0} km"
                       : $"Próximo cambio de aceite en {eval.DaysRemaining} días");

            yield return Alert(MaintenanceAlertType.Oil, severity, oil.Vehicle, "Aceite", detail);
        }
    }

    // ─── Batería ─────────────────────────────────────────────────────────────────
    // El estado lo reporta el mecánico (último chequeo). Replace → crítico,
    // ReplaceSoon → atención. Good/Fair no alertan; sin chequeos, no hay estado.

    private static IEnumerable<MaintenanceAlertDto> BuildBatteryAlerts(IReadOnlyList<VehicleBattery> batteries)
    {
        foreach (var battery in batteries)
        {
            var lastCheck = battery.Checks
                .OrderByDescending(c => c.CheckedOn)
                .FirstOrDefault();
            if (lastCheck is null) continue;

            if (lastCheck.Status is not (BatteryStatus.Replace or BatteryStatus.ReplaceSoon))
                continue;

            var critical = lastCheck.Status == BatteryStatus.Replace;
            var detail = critical
                ? "Batería para reemplazar — no esperes a que falle"
                : "Cambiá la batería en el próximo service";

            yield return Alert(
                MaintenanceAlertType.Battery,
                critical ? MaintenanceAlertSeverity.Critical : MaintenanceAlertSeverity.Warning,
                battery.Vehicle, "Batería", detail);
        }
    }

    // ─── Helper ──────────────────────────────────────────────────────────────────

    private static MaintenanceAlertDto Alert(
        MaintenanceAlertType type, MaintenanceAlertSeverity severity,
        Vehicle vehicle, string title, string detail)
        => new(
            Type:         type,
            Severity:     severity,
            VehicleId:    vehicle.Id,
            LicensePlate: vehicle.LicensePlate,
            Brand:        vehicle.Brand,
            Model:        vehicle.Model,
            Title:        title,
            Detail:       detail);
}
