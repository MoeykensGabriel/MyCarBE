using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddServiceToWorkOrder;

public class AddServiceToWorkOrderCommandHandler : IRequestHandler<AddServiceToWorkOrderCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository      _workOrderRepository;
    private readonly ICatalogServiceRepository _catalogServiceRepository;
    private readonly IUnitOfWork               _unitOfWork;
    private readonly IMapper                   _mapper;

    public AddServiceToWorkOrderCommandHandler(
        IWorkOrderRepository      workOrderRepository,
        ICatalogServiceRepository catalogServiceRepository,
        IUnitOfWork               unitOfWork,
        IMapper                   mapper)
    {
        _workOrderRepository      = workOrderRepository;
        _catalogServiceRepository = catalogServiceRepository;
        _unitOfWork               = unitOfWork;
        _mapper                   = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(AddServiceToWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // No se agregan servicios a órdenes cerradas, ni en AwaitingApproval: el presupuesto
        // ya salió congelado — para modificarlo hay que volver a Diagnosing ("Modificar
        // presupuesto" / ReviseQuote). En Approved/InProgress el servicio entra como ADICIONAL:
        // nace Pending y no suma al total hasta que el cliente lo apruebe (DecideAdditionalItems).
        if (workOrder.CurrentStatus is WorkOrderStatus.AwaitingApproval
                                    or WorkOrderStatus.Completed
                                    or WorkOrderStatus.Delivered
                                    or WorkOrderStatus.Cancelled)
            throw new BadRequestException(
                $"No se pueden agregar servicios a una orden en estado '{workOrder.CurrentStatus}'.");

        var catalogService = await _catalogServiceRepository.GetByIdAsync(request.CatalogServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogService), request.CatalogServiceId);

        if (!catalogService.IsActive)
            throw new BadRequestException($"El servicio '{catalogService.Name}' no está activo en el catálogo.");

        // Snapshot catalog values at the moment of addition
        var line = new WorkOrderService
        {
            WorkOrderId                      = workOrder.Id,
            CatalogServiceId                 = catalogService.Id,
            NameSnapshot                     = catalogService.Name,
            DescriptionSnapshot              = catalogService.Description,
            PriceSnapshot                    = catalogService.DefaultPrice,
            EstimatedDurationMinutesSnapshot = catalogService.EstimatedDurationMinutes,
            Quantity                         = request.Quantity,
        };

        workOrder.Services.Add(line);
        workOrder.RecalculateTotalAmount();

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
