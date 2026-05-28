using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.RemovePartFromWorkOrder;

public class RemovePartFromWorkOrderCommandHandler
    : IRequestHandler<RemovePartFromWorkOrderCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public RemovePartFromWorkOrderCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(
        RemovePartFromWorkOrderCommand request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus != WorkOrderStatus.Diagnosing)
            throw new BadRequestException(
                $"Solo se pueden eliminar repuestos de una orden en estado 'Diagnosing'. Estado actual: '{workOrder.CurrentStatus}'.");

        var part = workOrder.Parts.FirstOrDefault(p => p.Id == request.PartId && !p.IsDeleted)
            ?? throw new NotFoundException(nameof(WorkOrderPart), request.PartId);

        if (part.FrozenAt.HasValue)
            throw new BadRequestException(
                "Este repuesto fue congelado al enviar el presupuesto y no se puede eliminar.");

        // Soft delete — interceptado por AppDbContext.SaveChangesAsync
        part.IsDeleted = true;
        part.DeletedAt = DateTime.UtcNow;

        workOrder.RecalculateTotalAmount();

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-fetch para devolver el detail sin el item borrado (los Include filtran por IsDeleted)
        var updated = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken);
        return _mapper.Map<WorkOrderDetailDto>(updated!);
    }
}
