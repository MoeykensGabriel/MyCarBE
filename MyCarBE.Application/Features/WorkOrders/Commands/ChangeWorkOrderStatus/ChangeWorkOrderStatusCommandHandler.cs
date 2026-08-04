using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.StockRequests.Services;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ChangeWorkOrderStatus;

public class ChangeWorkOrderStatusCommandHandler : IRequestHandler<ChangeWorkOrderStatusCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository       _workOrderRepository;
    private readonly ICurrentUserService        _currentUser;
    private readonly IUnitOfWork                _unitOfWork;
    private readonly IMapper                    _mapper;
    private readonly IStockRequestOrchestrator  _stockRequestOrchestrator;

    public ChangeWorkOrderStatusCommandHandler(
        IWorkOrderRepository       workOrderRepository,
        ICurrentUserService        currentUser,
        IUnitOfWork                unitOfWork,
        IMapper                    mapper,
        IStockRequestOrchestrator  stockRequestOrchestrator)
    {
        _workOrderRepository      = workOrderRepository;
        _currentUser              = currentUser;
        _unitOfWork               = unitOfWork;
        _mapper                   = mapper;
        _stockRequestOrchestrator = stockRequestOrchestrator;
    }

    public async Task<WorkOrderDetailDto> Handle(ChangeWorkOrderStatusCommand request, CancellationToken cancellationToken)
    {
        // El envío del presupuesto tiene side effects propios (congelar items, generar token,
        // setear QuoteExpiresAt, mandar email). Forzamos a usar el endpoint dedicado para
        // que esos invariantes nunca se rompan vía esta ruta genérica.
        if (request.NewStatus == WorkOrderStatus.AwaitingApproval)
            throw new BadRequestException(
                "Para enviar el presupuesto al cliente usá POST /api/work-orders/{id}/send-quote en lugar de cambiar el estado directamente.");

        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), request.WorkOrderId);

        // La vuelta AwaitingApproval → Diagnosing ("Modificar presupuesto") también tiene side
        // effects propios (descongelar items, resetear decisión, invalidar token). Endpoint dedicado.
        if (workOrder.CurrentStatus == WorkOrderStatus.AwaitingApproval &&
            request.NewStatus == WorkOrderStatus.Diagnosing)
            throw new BadRequestException(
                "Para modificar un presupuesto enviado usá POST /api/work-orders/{id}/revise-quote en lugar de cambiar el estado directamente.");

        // Cerrar una orden de solo inspección valida que todas las áreas estén cubiertas.
        // Por esta ruta genérica ese chequeo se saltearía.
        if (workOrder.CurrentStatus == WorkOrderStatus.UnderInspection &&
            request.NewStatus == WorkOrderStatus.Completed)
            throw new BadRequestException(
                "Para cerrar una orden de solo inspección usá POST /api/work-orders/{id}/close-inspection en lugar de cambiar el estado directamente.");

        // La promoción setea PromotedToRepairAt; por acá el estado cambiaría sin esa marca
        // y la orden quedaría en Diagnosing pero seguiría contando como solo inspección.
        if (workOrder.CurrentStatus == WorkOrderStatus.Completed &&
            request.NewStatus == WorkOrderStatus.Diagnosing)
            throw new BadRequestException(
                "Para promover una inspección a orden de trabajo usá POST /api/work-orders/{id}/promote-to-work-order en lugar de cambiar el estado directamente.");

        try
        {
            // Shortcut del admin: si la WO está AwaitingApproval y el admin la mueve a
            // Approved/InProgress, se interpreta como "admin aprobó en nombre del cliente".
            // Sin esto, los items quedan en ApprovalStatus.Pending y los servicios nunca
            // aparecen en el pool de mecánicos (que filtra por Approved).
            if (workOrder.CurrentStatus == WorkOrderStatus.AwaitingApproval &&
                (request.NewStatus == WorkOrderStatus.Approved || request.NewStatus == WorkOrderStatus.InProgress))
            {
                var serviceIds = workOrder.Services.Where(s => !s.IsDeleted).Select(s => s.Id);
                var partIds    = workOrder.Parts.Where(p => !p.IsDeleted).Select(p => p.Id);
                workOrder.ApplyCustomerApproval(serviceIds, partIds);
            }

            workOrder.ChangeStatus(request.NewStatus, _currentUser.UserId, request.Note);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        // Si la WO acaba de quedar Approved o InProgress, generar pedido al depósito.
        // El orchestrator es idempotente: si ya existe un pedido para esta WO, no hace nada.
        if (request.NewStatus == WorkOrderStatus.Approved ||
            request.NewStatus == WorkOrderStatus.InProgress)
        {
            await _stockRequestOrchestrator.EnsureStockRequestForApprovedWorkOrderAsync(workOrder, cancellationToken);
        }

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
