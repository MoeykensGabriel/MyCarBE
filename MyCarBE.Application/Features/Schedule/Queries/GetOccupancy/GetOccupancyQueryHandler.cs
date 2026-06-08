using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Schedule.DTOs;

namespace MyCarBE.Application.Features.Schedule.Queries.GetOccupancy;

public class GetOccupancyQueryHandler : IRequestHandler<GetOccupancyQuery, OccupancyDto>
{
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly IWorkshopSettingsRepository _settingsRepository;

    public GetOccupancyQueryHandler(
        IWorkOrderRepository        workOrderRepository,
        IWorkshopSettingsRepository settingsRepository)
    {
        _workOrderRepository = workOrderRepository;
        _settingsRepository  = settingsRepository;
    }

    public async Task<OccupancyDto> Handle(GetOccupancyQuery request, CancellationToken cancellationToken)
    {
        // Normalizamos a día completo: [from 00:00 - to 23:59:59].
        var from = request.From.Date;
        var to   = request.To.Date.AddDays(1).AddTicks(-1);

        var orders   = await _workOrderRepository.GetScheduledWorkOrdersAsync(from, to, cancellationToken);
        var settings = await _settingsRepository.GetAsync(cancellationToken);

        var slots = orders.Select(w => new OccupancySlotDto(
            WorkOrderId:         w.Id,
            ScheduledStart:      w.ScheduledStart!.Value,
            ScheduledEnd:        w.ScheduledEnd!.Value,
            Status:              w.CurrentStatus,
            VehicleId:           w.Vehicle.Id,
            VehicleLicensePlate: w.Vehicle.LicensePlate,
            VehicleBrand:        w.Vehicle.Brand,
            VehicleModel:        w.Vehicle.Model,
            OwnerName:           w.CustomerAtEntry != null
                                    ? $"{w.CustomerAtEntry.FirstName} {w.CustomerAtEntry.LastName}".Trim()
                                    : w.FleetAtEntry?.CompanyName
        )).ToList();

        return new OccupancyDto(settings.PhysicalCapacity, slots);
    }
}
