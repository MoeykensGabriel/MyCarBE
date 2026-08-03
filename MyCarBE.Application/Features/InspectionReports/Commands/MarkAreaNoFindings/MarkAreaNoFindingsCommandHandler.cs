using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.InspectionReports.Commands.MarkAreaNoFindings;

public class MarkAreaNoFindingsCommandHandler : IRequestHandler<MarkAreaNoFindingsCommand, InspectionReportDto>
{
    private readonly IInspectionReportRepository _repository;
    private readonly IAreaRepository             _areaRepository;
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly IUnitOfWork                 _unitOfWork;
    private readonly IMapper                     _mapper;

    public MarkAreaNoFindingsCommandHandler(
        IInspectionReportRepository repository,
        IAreaRepository             areaRepository,
        IWorkOrderRepository        workOrderRepository,
        IUnitOfWork                 unitOfWork,
        IMapper                     mapper)
    {
        _repository          = repository;
        _areaRepository      = areaRepository;
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<InspectionReportDto> Handle(MarkAreaNoFindingsCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Igual que CreateInspectionReport: canal inicial (UnderInspection) o canal TARDÍO
        // sobre un área que quedó postergada. Acá el especialista miró y no encontró nada,
        // que es lo que salda la deuda sin generar trabajo.
        var isInitialInspection = workOrder.CurrentStatus == WorkOrderStatus.UnderInspection;

        var area = await _areaRepository.GetByIdAsync(request.AreaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Area), request.AreaId);

        if (!isInitialInspection)
        {
            if (!workOrder.AcceptsLateInspection)
                throw new BadRequestException(
                    $"No se puede marcar 'sin hallazgos' con la orden en '{workOrder.CurrentStatus}'.");

            if (!await _repository.IsAreaPendingForVehicleAsync(workOrder.VehicleId, request.AreaId, cancellationToken))
                throw new BadRequestException(
                    $"Con la inspección ya cerrada, el área '{area.Name}' solo se puede cerrar si había quedado postergada.");
        }

        // Un reporte vivo por (orden, área). En el canal tardío damos de baja la postergación
        // que venimos a reemplazar — el índice único es parcial sobre IsDeleted.
        var existing = await _repository.GetByWorkOrderAndAreaAsync(request.WorkOrderId, request.AreaId, cancellationToken);
        if (existing is not null)
        {
            if (isInitialInspection || !existing.IsSkipped)
                throw new ConflictException(
                    nameof(InspectionReport),
                    "AreaId",
                    $"Ya existe un reporte para el área '{area.Name}' en esta orden.");

            _repository.Delete(existing);
        }

        var report = new InspectionReport
        {
            Id           = Guid.NewGuid(),
            WorkOrderId  = workOrder.Id,
            AreaId       = area.Id,
            MechanicId   = null,
            Findings     = null,
            HasIssue     = false,
            IsNoFindings = true,
            IsLate       = !isInitialInspection,
        };

        await _repository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _repository.GetByWorkOrderAndAreaAsync(workOrder.Id, area.Id, cancellationToken);
        return _mapper.Map<InspectionReportDto>(saved!);
    }
}
