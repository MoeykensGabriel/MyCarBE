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

        if (workOrder.CurrentStatus != WorkOrderStatus.UnderInspection)
            throw new BadRequestException(
                $"Solo se puede marcar 'sin hallazgos' en órdenes con estado UnderInspection " +
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
            Id           = Guid.NewGuid(),
            WorkOrderId  = workOrder.Id,
            AreaId       = area.Id,
            MechanicId   = null,
            Findings     = null,
            HasIssue     = false,
            IsNoFindings = true,
        };

        await _repository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _repository.GetByWorkOrderAndAreaAsync(workOrder.Id, area.Id, cancellationToken);
        return _mapper.Map<InspectionReportDto>(saved!);
    }
}
