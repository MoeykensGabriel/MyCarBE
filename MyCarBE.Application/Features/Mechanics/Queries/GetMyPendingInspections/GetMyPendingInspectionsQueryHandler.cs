using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMyPendingInspections;

public class GetMyPendingInspectionsQueryHandler : IRequestHandler<GetMyPendingInspectionsQuery, IReadOnlyList<PendingInspectionDto>>
{
    private readonly IMechanicRepository  _mechanicRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ICurrentUserService  _currentUser;

    public GetMyPendingInspectionsQueryHandler(
        IMechanicRepository  mechanicRepository,
        IWorkOrderRepository workOrderRepository,
        ICurrentUserService  currentUser)
    {
        _mechanicRepository  = mechanicRepository;
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
    }

    public async Task<IReadOnlyList<PendingInspectionDto>> Handle(GetMyPendingInspectionsQuery request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException("Solo los mecánicos pueden consultar inspecciones pendientes.");

        var mechanic = await _mechanicRepository.GetByIdWithAreasAsync(mechanicId, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), mechanicId);

        if (!mechanic.IsActive)
            return Array.Empty<PendingInspectionDto>();

        // Áreas del mecánico (activas; las inactivas se ignoran a propósito)
        var myAreaIds = mechanic.Areas.Where(a => a.IsActive).Select(a => a.Id).ToHashSet();
        if (myAreaIds.Count == 0)
            return Array.Empty<PendingInspectionDto>();

        // Areas también necesita ser un diccionario para resolver el nombre / flag en el DTO
        var myAreasById = mechanic.Areas
            .Where(a => a.IsActive)
            .ToDictionary(a => a.Id, a => a);

        var workOrders = await _workOrderRepository.GetUnderInspectionWithReportsAsync(cancellationToken);

        var result = new List<PendingInspectionDto>(workOrders.Count);

        foreach (var wo in workOrders)
        {
            // Áreas ya cubiertas por algún reporte (de mecánico o sin hallazgos)
            var coveredAreaIds = wo.InspectionReports.Select(r => r.AreaId).ToHashSet();

            // Áreas del mecánico que aún no fueron cubiertas
            var pendingForMe = myAreaIds.Where(id => !coveredAreaIds.Contains(id)).ToList();
            if (pendingForMe.Count == 0)
                continue;

            var pendingAreas = pendingForMe
                .Select(id => new PendingInspectionAreaDto(id, myAreasById[id].Name, myAreasById[id].IsTireArea, myAreasById[id].IsBatteryArea))
                .OrderBy(a => a.AreaName)
                .ToList();

            var ownerName =
                wo.CustomerAtEntry is not null ? $"{wo.CustomerAtEntry.FirstName} {wo.CustomerAtEntry.LastName}".Trim()
                : wo.FleetAtEntry is not null ? wo.FleetAtEntry.CompanyName
                : null;

            result.Add(new PendingInspectionDto(
                WorkOrderId:         wo.Id,
                WorkOrderCreatedAt:  wo.CreatedAt,
                ServiceReason:       wo.ServiceReason,
                VehicleId:           wo.Vehicle.Id,
                VehicleBrand:        wo.Vehicle.Brand,
                VehicleModel:        wo.Vehicle.Model,
                VehicleLicensePlate: wo.Vehicle.LicensePlate,
                OwnerName:           ownerName,
                PendingAreas:        pendingAreas
            ));
        }

        return result;
    }
}
