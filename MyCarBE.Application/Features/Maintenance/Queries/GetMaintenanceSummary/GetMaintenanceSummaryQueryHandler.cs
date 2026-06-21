using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Maintenance.DTOs;

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
    private readonly ICurrentUserService         _currentUser;

    public GetMaintenanceSummaryQueryHandler(
        IMaintenanceAlertRepository alertRepository,
        ICurrentUserService         currentUser)
    {
        _alertRepository = alertRepository;
        _currentUser     = currentUser;
    }

    public async Task<IReadOnlyList<MaintenanceAlertDto>> Handle(
        GetMaintenanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var (customerId, fleetId) = ResolveOwner();
        var now    = DateTime.UtcNow;
        var alerts = await _alertRepository.GetActiveByOwnerAsync(customerId, fleetId, cancellationToken);

        var result = new List<MaintenanceAlertDto>();
        foreach (var alert in alerts)
        {
            var eval = MaintenanceAlertStatusCalculator.Evaluate(alert, alert.Vehicle.CurrentMileage, now);
            if (eval.Severity is null) continue; // todavía no vence → no alerta

            result.Add(new MaintenanceAlertDto(
                Id:           alert.Id,
                Type:         (MaintenanceAlertType)(int)alert.ItemType,
                Severity:     eval.Severity.Value,
                VehicleId:    alert.Vehicle.Id,
                LicensePlate: alert.Vehicle.LicensePlate,
                Brand:        alert.Vehicle.Brand,
                Model:        alert.Vehicle.Model,
                Title:        alert.Title,
                Detail:       BuildDetail(eval)));
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
