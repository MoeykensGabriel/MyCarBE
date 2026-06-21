using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Maintenance.Commands.SetVehicleMaintenanceAlerts;

public class SetVehicleMaintenanceAlertsCommandHandler
    : IRequestHandler<SetVehicleMaintenanceAlertsCommand, IReadOnlyList<MaintenanceAlertConfigDto>>
{
    private readonly IVehicleRepository          _vehicleRepository;
    private readonly IMaintenanceAlertRepository _alertRepository;
    private readonly IUnitOfWork                 _unitOfWork;

    public SetVehicleMaintenanceAlertsCommandHandler(
        IVehicleRepository          vehicleRepository,
        IMaintenanceAlertRepository alertRepository,
        IUnitOfWork                 unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _alertRepository   = alertRepository;
        _unitOfWork        = unitOfWork;
    }

    public async Task<IReadOnlyList<MaintenanceAlertConfigDto>> Handle(
        SetVehicleMaintenanceAlertsCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        var now      = DateTime.UtcNow;
        var existing = await _alertRepository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        var byId     = existing.ToDictionary(a => a.Id);
        var keptIds  = new HashSet<Guid>();

        foreach (var item in request.Items)
        {
            var title = string.IsNullOrWhiteSpace(item.Title)
                ? MaintenanceAlertMappings.DefaultTitle(item.ItemType)
                : item.Title.Trim();
            var description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim();

            if (item.Id is { } id && byId.TryGetValue(id, out var alert))
            {
                // Editar preservando la línea base (no reinicia el ciclo).
                alert.ItemType       = item.ItemType;
                alert.Title          = title;
                alert.Description    = description;
                alert.IntervalKm     = item.IntervalKm;
                alert.IntervalMonths = item.IntervalMonths;
                alert.Validate();
                keptIds.Add(id);
            }
            else
            {
                // Nuevo: línea base = km/fecha actuales. NO setear Id (lo asigna SaveChangesAsync).
                var nuevo = new MaintenanceAlert
                {
                    VehicleId       = vehicle.Id,
                    ItemType        = item.ItemType,
                    Title           = title,
                    Description     = description,
                    IntervalKm      = item.IntervalKm,
                    IntervalMonths  = item.IntervalMonths,
                    BaselineMileage = vehicle.CurrentMileage,
                    BaselineDate    = now,
                };
                nuevo.Validate();
                await _alertRepository.AddAsync(nuevo, cancellationToken);
            }
        }

        // Soft-delete de los que ya no están en la lista.
        foreach (var alert in existing)
            if (!keptIds.Contains(alert.Id))
                _alertRepository.Delete(alert);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var current = await _alertRepository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        return current.Select(a => a.ToConfigDto(vehicle.CurrentMileage, now)).ToList();
    }
}
