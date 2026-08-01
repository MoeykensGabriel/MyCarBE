using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.InspectionReports.Queries.GetVehicleSkippedInspections;

public class GetVehicleSkippedInspectionsQueryHandler
    : IRequestHandler<GetVehicleSkippedInspectionsQuery, IReadOnlyList<SkippedInspectionAreaDto>>
{
    private readonly IVehicleRepository          _vehicleRepository;
    private readonly IInspectionReportRepository _inspectionReportRepository;

    public GetVehicleSkippedInspectionsQueryHandler(
        IVehicleRepository          vehicleRepository,
        IInspectionReportRepository inspectionReportRepository)
    {
        _vehicleRepository          = vehicleRepository;
        _inspectionReportRepository = inspectionReportRepository;
    }

    public async Task<IReadOnlyList<SkippedInspectionAreaDto>> Handle(
        GetVehicleSkippedInspectionsQuery request, CancellationToken cancellationToken)
    {
        _ = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        var skipped = await _inspectionReportRepository
            .GetPendingSkippedForVehicleAsync(request.VehicleId, cancellationToken);

        return skipped
            .Select(r => new SkippedInspectionAreaDto(
                WorkOrderId:        r.WorkOrderId,
                WorkOrderCreatedAt: r.WorkOrder.CreatedAt,
                AreaId:             r.AreaId,
                AreaName:           r.Area.Name,
                SkipReason:         r.SkipReason ?? string.Empty,
                SkippedAt:          r.CreatedAt))
            .ToList();
    }
}
