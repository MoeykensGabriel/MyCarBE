using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.InspectionReports.Commands.CreateInspectionReport;

public class CreateInspectionReportCommandHandler : IRequestHandler<CreateInspectionReportCommand, InspectionReportDto>
{
    private readonly IInspectionReportRepository _repository;
    private readonly IMechanicRepository         _mechanicRepository;
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly ICurrentUserService         _currentUser;
    private readonly IUnitOfWork                 _unitOfWork;
    private readonly IMapper                     _mapper;

    public CreateInspectionReportCommandHandler(
        IInspectionReportRepository repository,
        IMechanicRepository         mechanicRepository,
        IWorkOrderRepository        workOrderRepository,
        ICurrentUserService         currentUser,
        IUnitOfWork                 unitOfWork,
        IMapper                     mapper)
    {
        _repository          = repository;
        _mechanicRepository  = mechanicRepository;
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<InspectionReportDto> Handle(CreateInspectionReportCommand request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException("Solo los mecánicos pueden crear reportes de inspección.");

        // Mecánico con sus áreas eager-loaded
        var mechanic = await _mechanicRepository.GetByIdWithAreasAsync(mechanicId, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), mechanicId);

        if (!mechanic.IsActive)
            throw new ForbiddenException("Mecánico desactivado no puede reportar inspecciones.");

        // Validar que el mecánico esté asignado al área del reporte
        if (!mechanic.Areas.Any(a => a.Id == request.AreaId))
            throw new ForbiddenException("No estás asignado al área de este reporte.");

        // Validar que la orden exista y esté en fase de inspección
        var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus != Domain.Enums.WorkOrderStatus.UnderInspection)
            throw new BadRequestException(
                $"Solo se puede reportar inspección en órdenes con estado UnderInspection. " +
                $"Estado actual: {workOrder.CurrentStatus}.");

        // Validar que no exista ya un reporte para esta (orden, área) — respeta el unique index a nivel app
        if (await _repository.ExistsForAreaAsync(request.WorkOrderId, request.AreaId, cancellationToken))
            throw new ConflictException(
                nameof(InspectionReport),
                "AreaId",
                $"Ya existe un reporte para esta área en la orden {request.WorkOrderId}.");

        var report = new InspectionReport
        {
            Id           = Guid.NewGuid(),
            WorkOrderId  = workOrder.Id,
            AreaId       = request.AreaId,
            MechanicId   = mechanic.Id,
            Findings     = string.IsNullOrWhiteSpace(request.Findings) ? null : request.Findings.Trim(),
            HasIssue     = request.HasIssue,
            IsNoFindings = false,
        };

        // Propuestas: solo si HasIssue=true tiene sentido. Si vienen con HasIssue=false las ignoramos.
        if (request.HasIssue)
        {
            if (request.ProposedServices is { Count: > 0 })
            {
                foreach (var ps in request.ProposedServices)
                {
                    report.ProposedServices.Add(new InspectionReportProposedService
                    {
                        Id                 = Guid.NewGuid(),
                        InspectionReportId = report.Id,
                        Name               = ps.Name.Trim(),
                        Description        = string.IsNullOrWhiteSpace(ps.Description) ? null : ps.Description.Trim(),
                        EstimatedLaborCost = ps.EstimatedLaborCost,
                        EstimatedDays      = ps.EstimatedDays,
                    });
                }
            }

            if (request.ProposedParts is { Count: > 0 })
            {
                foreach (var pp in request.ProposedParts)
                {
                    report.ProposedParts.Add(new InspectionReportProposedPart
                    {
                        Id                 = Guid.NewGuid(),
                        InspectionReportId = report.Id,
                        Name               = pp.Name.Trim(),
                        Quantity           = pp.Quantity,
                        ProductCode        = string.IsNullOrWhiteSpace(pp.ProductCode) ? null : pp.ProductCode.Trim(),
                        EstimatedUnitPrice = pp.EstimatedUnitPrice,
                    });
                }
            }
        }

        await _repository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload con Area/Mechanic para el DTO
        var saved = await _repository.GetByWorkOrderAndAreaAsync(workOrder.Id, request.AreaId, cancellationToken);
        return _mapper.Map<InspectionReportDto>(saved!);
    }
}
