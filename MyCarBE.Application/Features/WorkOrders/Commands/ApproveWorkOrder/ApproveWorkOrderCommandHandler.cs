using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.StockRequests.Services;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveWorkOrder;

public class ApproveWorkOrderCommandHandler : IRequestHandler<ApproveWorkOrderCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderApprovalTokenRepository _tokenRepository;
    private readonly IWorkOrderRepository              _workOrderRepository;
    private readonly IUnitOfWork                       _unitOfWork;
    private readonly IMapper                           _mapper;
    private readonly IStockRequestOrchestrator         _stockRequestOrchestrator;

    public ApproveWorkOrderCommandHandler(
        IWorkOrderApprovalTokenRepository tokenRepository,
        IWorkOrderRepository              workOrderRepository,
        IUnitOfWork                       unitOfWork,
        IMapper                           mapper,
        IStockRequestOrchestrator         stockRequestOrchestrator)
    {
        _tokenRepository          = tokenRepository;
        _workOrderRepository      = workOrderRepository;
        _unitOfWork               = unitOfWork;
        _mapper                   = mapper;
        _stockRequestOrchestrator = stockRequestOrchestrator;
    }

    public async Task<WorkOrderDetailDto> Handle(ApproveWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var approvalToken = await _tokenRepository.GetValidByTokenAsync(request.Token, cancellationToken)
            ?? throw new BadRequestException("El enlace de aprobación es inválido o ha expirado.");

        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(approvalToken.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), approvalToken.WorkOrderId);

        // Defensa extra: el token ya expira junto con el presupuesto, pero si esos TTL
        // divergen algún día, este chequeo mantiene la regla de negocio (14 días).
        if (workOrder.QuoteExpiresAt is { } expiresAt && expiresAt < DateTime.UtcNow)
            throw new BadRequestException(
                "El presupuesto venció (validez: 14 días). Pedí al taller que lo actualice y lo vuelva a enviar.");

        try
        {
            // Aplica decisión + valida grupos + recalcula total — todo en el dominio.
            workOrder.ApplyCustomerApproval(request.ApprovedServiceIds, request.ApprovedPartIds);

            // Transición de estado: AwaitingApproval → Approved.
            // Usuario sistema porque la acción vino por token público (no hay UserId).
            workOrder.ChangeStatus(
                WorkOrderStatus.Approved,
                Guid.Empty,
                "Aprobado por el cliente vía enlace.");
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        approvalToken.IsUsed = true;
        approvalToken.UsedAt = DateTime.UtcNow;

        // Crear el pedido al depósito antes del SaveChanges, así todo se commitea junto.
        await _stockRequestOrchestrator.EnsureStockRequestForApprovedWorkOrderAsync(workOrder, cancellationToken);

        _workOrderRepository.Update(workOrder);
        _tokenRepository.Update(approvalToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
