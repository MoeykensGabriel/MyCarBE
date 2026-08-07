using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMyPendingInspections;

public class GetMyPendingInspectionsQueryHandler : IRequestHandler<GetMyPendingInspectionsQuery, IReadOnlyList<PendingInspectionDto>>
{
    private readonly IMechanicRepository         _mechanicRepository;
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly IAreaRepository             _areaRepository;
    private readonly IInspectionReportRepository _inspectionReportRepository;
    private readonly ICurrentUserService         _currentUser;

    public GetMyPendingInspectionsQueryHandler(
        IMechanicRepository         mechanicRepository,
        IWorkOrderRepository        workOrderRepository,
        IAreaRepository             areaRepository,
        IInspectionReportRepository inspectionReportRepository,
        ICurrentUserService         currentUser)
    {
        _mechanicRepository         = mechanicRepository;
        _workOrderRepository        = workOrderRepository;
        _areaRepository             = areaRepository;
        _inspectionReportRepository = inspectionReportRepository;
        _currentUser                = currentUser;
    }

    public async Task<IReadOnlyList<PendingInspectionDto>> Handle(GetMyPendingInspectionsQuery request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException("Solo los mecánicos pueden consultar inspecciones pendientes.");

        var mechanic = await _mechanicRepository.GetByIdWithAreasAsync(mechanicId, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), mechanicId);

        if (!mechanic.IsActive)
            return Array.Empty<PendingInspectionDto>();

        // Áreas que le tocan al mecánico (activas):
        // - Generalista: TODAS las áreas activas (puede reportar cualquiera).
        // - Normal: solo las áreas que tiene asignadas.
        var relevantAreas = mechanic.IsGeneralist
            ? (await _areaRepository.GetAllAsync(includeInactive: false, cancellationToken)).ToList()
            : mechanic.Areas.Where(a => a.IsActive).ToList();

        var myAreaIds = relevantAreas.Select(a => a.Id).ToHashSet();
        if (myAreaIds.Count == 0)
            return Array.Empty<PendingInspectionDto>();

        // Diccionario para resolver el nombre / flags en el DTO
        var myAreasById = relevantAreas.ToDictionary(a => a.Id, a => a);

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
                .Select(id => new PendingInspectionAreaDto(id, myAreasById[id].Name, myAreasById[id].IsTireArea, myAreasById[id].IsBatteryArea, myAreasById[id].IsOilArea))
                .OrderBy(a => a.AreaName)
                .ToList();

            // Sin propietario/cliente/flota: el mecánico no debe saber para quién es el trabajo.
            result.Add(new PendingInspectionDto(
                WorkOrderId:         wo.Id,
                WorkOrderCreatedAt:  wo.CreatedAt,
                ServiceReason:       wo.ServiceReason,
                MileageAtEntry:      wo.MileageAtEntry,
                VehicleId:           wo.Vehicle.Id,
                VehicleBrand:        wo.Vehicle.Brand,
                VehicleModel:        wo.Vehicle.Model,
                VehicleLicensePlate: wo.Vehicle.LicensePlate,
                PendingAreas:        pendingAreas
            ));
        }

        // ── Inspecciones TARDÍAS ────────────────────────────────────────────────────
        // Órdenes que ya cerraron su inspección pero siguen vivas. Sin esto el canal de
        // inspección tardía existe pero el especialista nunca se entera: su panel solo
        // miraba órdenes en UnderInspection, y ahí un área postergada figura como
        // "cubierta" (tiene reporte), así que desaparecía de su lista igual.
        var lateOrders = await _workOrderRepository.GetLateInspectionWindowWithReportsAsync(cancellationToken);

        // La deuda es del VEHÍCULO y puede venir de una visita anterior, así que la
        // resolvemos con la misma consulta que usa el guard de escritura — lo que el
        // mecánico ve es exactamente lo que el backend le va a permitir cargar.
        // Cacheamos por vehículo: varias órdenes vivas pueden compartirlo.
        var debtByVehicle = new Dictionary<Guid, HashSet<Guid>>();

        foreach (var wo in lateOrders)
        {
            if (!debtByVehicle.TryGetValue(wo.VehicleId, out var debtAreaIds))
            {
                var debt = await _inspectionReportRepository
                    .GetPendingSkippedForVehicleAsync(wo.VehicleId, cancellationToken);
                debtAreaIds = debt.Select(r => r.AreaId).ToHashSet();
                debtByVehicle[wo.VehicleId] = debtAreaIds;
            }

            var pendingForMe = myAreaIds.Where(id => debtAreaIds.Contains(id)).ToList();
            if (pendingForMe.Count == 0)
                continue;

            var pendingAreas = pendingForMe
                .Select(id => new PendingInspectionAreaDto(id, myAreasById[id].Name, myAreasById[id].IsTireArea, myAreasById[id].IsBatteryArea, myAreasById[id].IsOilArea))
                .OrderBy(a => a.AreaName)
                .ToList();

            result.Add(new PendingInspectionDto(
                WorkOrderId:         wo.Id,
                WorkOrderCreatedAt:  wo.CreatedAt,
                ServiceReason:       wo.ServiceReason,
                MileageAtEntry:      wo.MileageAtEntry,
                VehicleId:           wo.Vehicle.Id,
                VehicleBrand:        wo.Vehicle.Brand,
                VehicleModel:        wo.Vehicle.Model,
                VehicleLicensePlate: wo.Vehicle.LicensePlate,
                PendingAreas:        pendingAreas,
                IsLateInspection:    true
            ));
        }

        return result;
    }
}
