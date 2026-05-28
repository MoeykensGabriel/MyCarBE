using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Schedule.DTOs;

namespace MyCarBE.Application.Features.Schedule.Queries.GetSchedule;

public class GetScheduleQueryHandler : IRequestHandler<GetScheduleQuery, IReadOnlyList<ScheduleSlotDto>>
{
    private readonly IWorkOrderRepository _repository;

    public GetScheduleQueryHandler(IWorkOrderRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ScheduleSlotDto>> Handle(GetScheduleQuery request, CancellationToken cancellationToken)
    {
        // Normalizamos los límites a día completo: [from 00:00 - to 23:59:59]
        var from = request.From.Date;
        var to   = request.To.Date.AddDays(1).AddTicks(-1);

        var services = await _repository.GetScheduledServicesAsync(from, to, cancellationToken);

        return services.Select(s => new ScheduleSlotDto(
            WorkOrderServiceId:  s.Id,
            WorkOrderId:         s.WorkOrderId,
            ServiceName:         s.NameSnapshot,
            ScheduledStart:      s.ScheduledStart!.Value,
            ScheduledEnd:        s.ScheduledEnd!.Value,
            EstimatedDays:       s.EstimatedDays,
            AreaId:              s.AreaId,
            AreaName:            s.Area?.Name,
            MechanicId:          s.AssignedMechanicId,
            MechanicFullName:    s.AssignedMechanic == null
                                    ? null
                                    : ($"{s.AssignedMechanic.FirstName} {s.AssignedMechanic.LastName}").Trim(),
            VehicleId:           s.WorkOrder.Vehicle.Id,
            VehicleLicensePlate: s.WorkOrder.Vehicle.LicensePlate,
            VehicleBrand:        s.WorkOrder.Vehicle.Brand,
            VehicleModel:        s.WorkOrder.Vehicle.Model
        )).ToList();
    }
}
