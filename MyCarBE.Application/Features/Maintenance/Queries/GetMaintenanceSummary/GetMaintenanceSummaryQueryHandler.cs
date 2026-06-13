using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Application.Features.VehicleTires; // TireWearCalculator (regla compartida)
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Maintenance.Queries.GetMaintenanceSummary;

public class GetMaintenanceSummaryQueryHandler
    : IRequestHandler<GetMaintenanceSummaryQuery, IReadOnlyList<MaintenanceAlertDto>>
{
    private readonly IVehicleTireRepository _tireRepository;
    private readonly ICurrentUserService    _currentUser;

    public GetMaintenanceSummaryQueryHandler(
        IVehicleTireRepository tireRepository,
        ICurrentUserService    currentUser)
    {
        _tireRepository = tireRepository;
        _currentUser    = currentUser;
    }

    public async Task<IReadOnlyList<MaintenanceAlertDto>> Handle(
        GetMaintenanceSummaryQuery request, CancellationToken cancellationToken)
    {
        // El dueño sale del JWT: una flota ve los autos de la flota; un particular los suyos.
        Guid? customerId, fleetId;
        if (_currentUser.FleetId.HasValue)
        {
            fleetId    = _currentUser.FleetId;
            customerId = null;
        }
        else
        {
            customerId = _currentUser.CustomerId;
            fleetId    = null;
        }

        var tires = await _tireRepository.GetActiveTiresByOwnerAsync(customerId, fleetId, cancellationToken);

        var alerts = new List<MaintenanceAlertDto>();

        // Una alerta por vehículo: agrupa sus cubiertas y resume el peor estado.
        foreach (var group in tires.GroupBy(t => t.VehicleId))
        {
            var vehicle = group.First().Vehicle;
            int urgent = 0, replaceSoon = 0, irregular = 0;

            foreach (var tire in group)
            {
                // Misma regla que la card del detalle — deriva estado de la última medición.
                var estimation = TireWearCalculator.Calculate(tire);
                if      (estimation.Status == TireStatus.Urgent)      urgent++;
                else if (estimation.Status == TireStatus.ReplaceSoon) replaceSoon++;
                if (estimation.HasIrregularWear) irregular++;
            }

            // Attention y Healthy no alertan (decisión de producto: no saturar el Inicio).
            if (urgent == 0 && replaceSoon == 0 && irregular == 0) continue;

            alerts.Add(new MaintenanceAlertDto(
                Type:         MaintenanceAlertType.Tire,
                Severity:     urgent > 0 ? MaintenanceAlertSeverity.Critical : MaintenanceAlertSeverity.Warning,
                VehicleId:    vehicle.Id,
                LicensePlate: vehicle.LicensePlate,
                Brand:        vehicle.Brand,
                Model:        vehicle.Model,
                Title:        "Cubiertas",
                Detail:       BuildTireDetail(urgent, replaceSoon, irregular)));
        }

        // Críticas primero; desempate estable por patente.
        return alerts
            .OrderByDescending(a => a.Severity)
            .ThenBy(a => a.LicensePlate)
            .ToList();
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
}
