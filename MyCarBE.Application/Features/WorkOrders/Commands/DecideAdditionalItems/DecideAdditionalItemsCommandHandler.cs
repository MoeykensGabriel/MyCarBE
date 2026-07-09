using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.StockRequests.Services;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.WorkOrders.Commands.DecideAdditionalItems;

public class DecideAdditionalItemsCommandHandler
    : IRequestHandler<DecideAdditionalItemsCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository      _workOrderRepository;
    private readonly ICurrentUserService       _currentUser;
    private readonly IUnitOfWork               _unitOfWork;
    private readonly IMapper                   _mapper;
    private readonly IStockRequestOrchestrator _stockRequestOrchestrator;

    public DecideAdditionalItemsCommandHandler(
        IWorkOrderRepository      workOrderRepository,
        ICurrentUserService       currentUser,
        IUnitOfWork               unitOfWork,
        IMapper                   mapper,
        IStockRequestOrchestrator stockRequestOrchestrator)
    {
        _workOrderRepository      = workOrderRepository;
        _currentUser              = currentUser;
        _unitOfWork               = unitOfWork;
        _mapper                   = mapper;
        _stockRequestOrchestrator = stockRequestOrchestrator;
    }

    public async Task<WorkOrderDetailDto> Handle(
        DecideAdditionalItemsCommand request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Aprobar un repuesto de depósito dispara un pedido a GestionPGB con el snapshot de
        // la condición de venta. Si la orden todavía no la tiene (ej: el presupuesto original
        // era solo mano de obra), hay que cargarla antes de aprobar el adicional.
        var approvingDepotParts = workOrder.Parts.Any(p =>
            !p.IsDeleted &&
            request.ApprovedPartIds.Contains(p.Id) &&
            !string.IsNullOrWhiteSpace(p.ProductCode));

        if (approvingDepotParts && workOrder.SaleCondition is null)
            throw new BadRequestException(
                "Estás aprobando repuestos de depósito: cargá la condición de venta " +
                "(cuenta corriente / orden de compra / contado) antes de registrar la aprobación.");

        try
        {
            // Valida estado + items Pending, aplica la decisión y recalcula el total.
            workOrder.ApplyAdditionalDecision(
                request.ApprovedServiceIds,
                request.RejectedServiceIds,
                request.ApprovedPartIds,
                request.RejectedPartIds);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        // Si se aprobaron repuestos nuevos, va un pedido adicional al depósito. El
        // orchestrator es delta-based: solo pide lo aprobado que todavía no se pidió.
        if (request.ApprovedPartIds.Count > 0)
            await _stockRequestOrchestrator.EnsureStockRequestForApprovedWorkOrderAsync(workOrder, cancellationToken);

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
