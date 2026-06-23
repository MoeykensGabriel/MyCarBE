using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ConvertInspectionProposals;

public class ConvertInspectionProposalsCommandHandler
    : IRequestHandler<ConvertInspectionProposalsCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly IInspectionReportRepository _inspectionRepository;
    private readonly IUnitOfWork                 _unitOfWork;
    private readonly IMapper                     _mapper;

    public ConvertInspectionProposalsCommandHandler(
        IWorkOrderRepository        workOrderRepository,
        IInspectionReportRepository inspectionRepository,
        IUnitOfWork                 unitOfWork,
        IMapper                     mapper)
    {
        _workOrderRepository  = workOrderRepository;
        _inspectionRepository = inspectionRepository;
        _unitOfWork           = unitOfWork;
        _mapper               = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(
        ConvertInspectionProposalsCommand request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus != WorkOrderStatus.Diagnosing)
            throw new BadRequestException(
                $"Solo se pueden consolidar propuestas en una orden en estado 'Diagnosing'. " +
                $"Estado actual: '{workOrder.CurrentStatus}'.");

        var reports = await _inspectionRepository.GetByWorkOrderWithProposalsAsync(workOrder.Id, cancellationToken);

        // Map proposalId → areaId del reporte origen, para arrastrar el área al WorkOrderService.
        var serviceProposalAreaMap = reports
            .SelectMany(r => r.ProposedServices.Select(s => new { s.Id, r.AreaId }))
            .ToDictionary(x => x.Id, x => x.AreaId);

        var allProposedServices = reports.SelectMany(r => r.ProposedServices).ToList();
        var allProposedParts    = reports.SelectMany(r => r.ProposedParts).ToList();

        var wantedSvcIds  = new HashSet<Guid>(request.ProposedServiceIds);
        var wantedPartIds = new HashSet<Guid>(request.ProposedPartIds);

        // Validar que todos los IDs pertenezcan a alguna propuesta de esta WO
        var validSvcIds  = allProposedServices.Select(s => s.Id).ToHashSet();
        var validPartIds = allProposedParts.Select(p => p.Id).ToHashSet();

        if (wantedSvcIds.Any(id => !validSvcIds.Contains(id)))
            throw new BadRequestException("Hay propuestas de servicios que no pertenecen a esta orden.");

        if (wantedPartIds.Any(id => !validPartIds.Contains(id)))
            throw new BadRequestException("Hay propuestas de repuestos que no pertenecen a esta orden.");

        if (wantedSvcIds.Count == 0 && wantedPartIds.Count == 0)
            throw new BadRequestException("Seleccioná al menos una propuesta para convertir al presupuesto.");

        foreach (var proposal in allProposedServices.Where(s => wantedSvcIds.Contains(s.Id)))
        {
            workOrder.Services.Add(new WorkOrderService
            {
                WorkOrderId                      = workOrder.Id,
                CatalogServiceId                 = null, // ad-hoc; vino del mecánico
                AreaId                           = serviceProposalAreaMap[proposal.Id],
                NameSnapshot                     = proposal.Name,
                DescriptionSnapshot              = proposal.Description ?? string.Empty,
                PriceSnapshot                    = proposal.EstimatedLaborCost,
                EstimatedDurationMinutesSnapshot = 0,
                Quantity                         = 1,
                ApprovalStatus                   = QuoteItemApprovalStatus.Pending,
                EstimatedDurationMinutes         = proposal.EstimatedDurationMinutes,
            });
        }

        foreach (var proposal in allProposedParts.Where(p => wantedPartIds.Contains(p.Id)))
        {
            workOrder.Parts.Add(new WorkOrderPart
            {
                WorkOrderId    = workOrder.Id,
                ProductCode    = proposal.ProductCode,
                Name           = proposal.Name,
                UnitPrice      = proposal.EstimatedUnitPrice ?? 0m, // si el mecánico no sabía, queda 0 — la oficina lo completa
                Quantity       = proposal.Quantity,
                ApprovalStatus = QuoteItemApprovalStatus.Pending,
            });
        }

        workOrder.RecalculateTotalAmount();

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
