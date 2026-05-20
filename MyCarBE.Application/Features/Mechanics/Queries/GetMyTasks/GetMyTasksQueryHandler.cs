using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMyTasks;

public class GetMyTasksQueryHandler : IRequestHandler<GetMyTasksQuery, IReadOnlyList<MechanicTaskDto>>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ICurrentUserService  _currentUser;

    public GetMyTasksQueryHandler(
        IWorkOrderRepository workOrderRepository,
        ICurrentUserService  currentUser)
    {
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
    }

    public async Task<IReadOnlyList<MechanicTaskDto>> Handle(GetMyTasksQuery request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException("Solo los mecánicos pueden ver sus tareas asignadas.");

        var services = await _workOrderRepository.GetServicesByMechanicAsync(
            mechanicId, request.Status, cancellationToken);

        return services.Select(s => new MechanicTaskDto(
            WorkOrderServiceId:     s.Id,
            WorkOrderId:            s.WorkOrderId,
            VehicleId:              s.WorkOrder.VehicleId,
            VehicleBrand:           s.WorkOrder.Vehicle.Brand,
            VehicleModel:           s.WorkOrder.Vehicle.Model,
            VehicleLicensePlate:    s.WorkOrder.Vehicle.LicensePlate,
            WorkOrderCurrentStatus: s.WorkOrder.CurrentStatus,
            ServiceName:            s.NameSnapshot,
            ServiceDescription:     s.DescriptionSnapshot,
            Quantity:               s.Quantity,
            AssignmentStatus:       s.AssignmentStatus,
            AcceptedAt:             s.AcceptedAt,
            CompletedAt:            s.CompletedAt,
            CustomerNote:           s.WorkOrder.CustomerNote,
            TechnicianNote:         s.WorkOrder.TechnicianNote,
            MechanicNotes:          s.MechanicNotes,
            MechanicFindings:       s.MechanicFindings,
            UpdatedAt:              s.UpdatedAt
        )).ToList();
    }
}
