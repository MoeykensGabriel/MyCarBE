using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.SetSaleCondition;

public class SetSaleConditionCommandHandler : IRequestHandler<SetSaleConditionCommand, WorkOrderDetailDto>
{
    // Editable solo mientras el pedido al depósito no se disparó (se dispara al aprobar).
    private static readonly WorkOrderStatus[] EditableStatuses =
    {
        WorkOrderStatus.Received,
        WorkOrderStatus.UnderInspection,
        WorkOrderStatus.Diagnosing,
        WorkOrderStatus.AwaitingApproval,
    };

    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public SetSaleConditionCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(SetSaleConditionCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (!EditableStatuses.Contains(workOrder.CurrentStatus))
            throw new BadRequestException(
                "La condición de venta ya no se puede modificar: el pedido al depósito se envía al aprobar " +
                $"y esta orden está en estado {workOrder.CurrentStatus}.");

        workOrder.SaleCondition = request.Condition;

        // Los datos acompañantes solo tienen sentido para su condición; el resto se limpia
        // para no mandar información vieja al depósito.
        workOrder.PurchaseOrderNumber = request.Condition == SaleCondition.OrdenDeCompra
            ? request.PurchaseOrderNumber?.Trim()
            : null;
        workOrder.DepositAmount = request.Condition == SaleCondition.Contado
            ? request.DepositAmount
            : null;

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
