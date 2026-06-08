using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.InspectionReports.Commands.UpdateInspectionReport;

public class UpdateInspectionReportCommandHandler : IRequestHandler<UpdateInspectionReportCommand, InspectionReportDto>
{
    private readonly IInspectionReportRepository _repository;
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly ICurrentUserService         _currentUser;
    private readonly IUnitOfWork                 _unitOfWork;
    private readonly IMapper                     _mapper;

    public UpdateInspectionReportCommandHandler(
        IInspectionReportRepository repository,
        IWorkOrderRepository        workOrderRepository,
        ICurrentUserService         currentUser,
        IUnitOfWork                 unitOfWork,
        IMapper                     mapper)
    {
        _repository          = repository;
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<InspectionReportDto> Handle(UpdateInspectionReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByIdWithProposalsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(InspectionReport), request.Id);

        // Ownership: solo el mecánico que lo creó puede editarlo
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException("Solo los mecánicos pueden editar reportes de inspección.");

        if (report.MechanicId != mechanicId)
            throw new ForbiddenException("Solo el mecánico que creó el reporte puede editarlo.");

        // No se permite editar reportes "sin hallazgos" (los creó el admin, no un mecánico — defensa extra)
        if (report.IsNoFindings)
            throw new BadRequestException("Este reporte fue marcado como 'sin hallazgos' por el admin y no se edita.");

        // No se permite editar si la orden ya pasó la fase de inspección
        var workOrder = await _workOrderRepository.GetByIdAsync(report.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), report.WorkOrderId);

        if (workOrder.CurrentStatus != WorkOrderStatus.UnderInspection)
            throw new BadRequestException(
                $"No se puede editar el reporte: la orden ya no está en fase de inspección " +
                $"(estado actual: {workOrder.CurrentStatus}).");

        report.Findings = string.IsNullOrWhiteSpace(request.Findings) ? null : request.Findings.Trim();
        report.HasIssue = request.HasIssue;

        // Reemplazo total de propuestas. Si HasIssue=false las propuestas no tienen sentido — limpiamos.
        _repository.RemoveAllProposals(report);
        report.ProposedServices.Clear();
        report.ProposedParts.Clear();

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
                        EstimatedDurationMinutes = ps.EstimatedDurationMinutes,
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

        _repository.Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload con Area/Mechanic/Photos para el DTO
        var saved = await _repository.GetByWorkOrderAndAreaAsync(report.WorkOrderId, report.AreaId, cancellationToken);
        return _mapper.Map<InspectionReportDto>(saved!);
    }
}
