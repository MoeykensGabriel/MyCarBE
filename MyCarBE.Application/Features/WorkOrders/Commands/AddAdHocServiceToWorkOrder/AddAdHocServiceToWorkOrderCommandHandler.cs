using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddAdHocServiceToWorkOrder;

public class AddAdHocServiceToWorkOrderCommandHandler
    : IRequestHandler<AddAdHocServiceToWorkOrderCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public AddAdHocServiceToWorkOrderCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(
        AddAdHocServiceToWorkOrderCommand request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus is WorkOrderStatus.Completed
                                    or WorkOrderStatus.Delivered
                                    or WorkOrderStatus.Cancelled)
            throw new BadRequestException(
                $"No se pueden agregar servicios a una orden en estado '{workOrder.CurrentStatus}'.");

        // Servicio ad-hoc: CatalogServiceId queda null, los datos viven en los snapshots.
        var line = new WorkOrderService
        {
            WorkOrderId                      = workOrder.Id,
            CatalogServiceId                 = null,
            NameSnapshot                     = request.Name.Trim(),
            DescriptionSnapshot              = request.Description.Trim(),
            PriceSnapshot                    = request.Price,
            EstimatedDurationMinutesSnapshot = request.EstimatedDurationMinutes,
            Quantity                         = request.Quantity,
        };

        workOrder.Services.Add(line);
        workOrder.RecalculateTotalAmount();

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
