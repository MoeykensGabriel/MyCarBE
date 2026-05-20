using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveWorkOrder;

public class ApproveWorkOrderCommandHandler : IRequestHandler<ApproveWorkOrderCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderApprovalTokenRepository _tokenRepository;
    private readonly IWorkOrderRepository              _workOrderRepository;
    private readonly IUnitOfWork                       _unitOfWork;
    private readonly IMapper                           _mapper;

    public ApproveWorkOrderCommandHandler(
        IWorkOrderApprovalTokenRepository tokenRepository,
        IWorkOrderRepository              workOrderRepository,
        IUnitOfWork                       unitOfWork,
        IMapper                           mapper)
    {
        _tokenRepository     = tokenRepository;
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(ApproveWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var approvalToken = await _tokenRepository.GetValidByTokenAsync(request.Token, cancellationToken)
            ?? throw new BadRequestException("El enlace de aprobación es inválido o ha expirado.");

        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(approvalToken.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), approvalToken.WorkOrderId);

        if (workOrder.CurrentStatus != WorkOrderStatus.AwaitingApproval)
            throw new BadRequestException(
                $"La orden no está pendiente de aprobación. Estado actual: {workOrder.CurrentStatus}.");

        // Use a system/anonymous user ID for the status change event
        var systemUserId = Guid.Empty;
        try
        {
            // El cliente aprueba → pasa a Approved (todavía no In Progress).
            // El admin debe pasar manualmente a InProgress cuando el vehículo esté en el taller.
            workOrder.ChangeStatus(WorkOrderStatus.Approved, systemUserId, "Aprobado por el cliente vía enlace.");
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        approvalToken.IsUsed = true;
        approvalToken.UsedAt = DateTime.UtcNow;

        _workOrderRepository.Update(workOrder);
        _tokenRepository.Update(approvalToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
