using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.RemoveServiceFromWorkOrder;

public class RemoveServiceFromWorkOrderCommandHandler : IRequestHandler<RemoveServiceFromWorkOrderCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public RemoveServiceFromWorkOrderCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(RemoveServiceFromWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus is WorkOrderStatus.Completed
                                     or WorkOrderStatus.Delivered
                                     or WorkOrderStatus.Cancelled)
            throw new BadRequestException(
                $"No se pueden eliminar servicios de una orden en estado '{workOrder.CurrentStatus}'.");

        var service = workOrder.Services.FirstOrDefault(s => s.Id == request.WorkOrderServiceId)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrderService), request.WorkOrderServiceId);

        // Soft delete — intercepted by AppDbContext SaveChangesAsync
        service.IsDeleted  = true;
        service.DeletedAt  = DateTime.UtcNow;

        workOrder.RecalculateTotalAmount();
        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-map with the deleted service filtered out (GetWithFullDetails filters IsDeleted)
        var updated = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken);
        return _mapper.Map<WorkOrderDetailDto>(updated!);
    }
}
