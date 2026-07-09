using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.InspectionReports.Commands.MarkAreaSkipped;

public class MarkAreaSkippedCommandHandler : IRequestHandler<MarkAreaSkippedCommand, InspectionReportDto>
{
    private readonly IInspectionReportRepository _repository;
    private readonly IAreaRepository             _areaRepository;
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly IUnitOfWork                 _unitOfWork;
    private readonly IMapper                     _mapper;

    public MarkAreaSkippedCommandHandler(
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

    public async Task<InspectionReportDto> Handle(MarkAreaSkippedCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus != WorkOrderStatus.UnderInspection)
            throw new BadRequestException(
                $"Solo se puede omitir un área en órdenes con estado UnderInspection " +
                $"(estado actual: {workOrder.CurrentStatus}).");

        var area = await _areaRepository.GetByIdAsync(request.AreaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Area), request.AreaId);

        if (await _repository.ExistsForAreaAsync(request.WorkOrderId, request.AreaId, cancellationToken))
            throw new ConflictException(
                nameof(InspectionReport),
                "AreaId",
                $"Ya existe un reporte para el área '{area.Name}' en esta orden.");

        var report = new InspectionReport
        {
            WorkOrderId  = workOrder.Id,
            AreaId       = area.Id,
            MechanicId   = null,
            Findings     = null,
            HasIssue     = false,
            IsNoFindings = false,
            IsSkipped    = true,
            SkipReason   = request.Reason.Trim(),
        };

        await _repository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _repository.GetByWorkOrderAndAreaAsync(workOrder.Id, area.Id, cancellationToken);
        return _mapper.Map<InspectionReportDto>(saved!);
    }
}
