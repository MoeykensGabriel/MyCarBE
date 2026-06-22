using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderServicePrice;

public class UpdateWorkOrderServicePriceCommandHandler
    : IRequestHandler<UpdateWorkOrderServicePriceCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public UpdateWorkOrderServicePriceCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(
        UpdateWorkOrderServicePriceCommand request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus != WorkOrderStatus.Diagnosing)
            throw new BadRequestException(
                $"Solo se puede editar el precio de servicios en una orden en estado 'Diagnosing'. Estado actual: '{workOrder.CurrentStatus}'.");

        var service = workOrder.Services.FirstOrDefault(s => s.Id == request.ServiceId && !s.IsDeleted)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.ServiceId);

        if (service.FrozenAt.HasValue)
            throw new BadRequestException(
                "Este servicio fue congelado al enviar el presupuesto y no se puede modificar.");

        service.PriceSnapshot = request.Price;

        workOrder.RecalculateTotalAmount();

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
