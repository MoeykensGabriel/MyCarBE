using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddPartToWorkOrder;

public class AddPartToWorkOrderCommandHandler
    : IRequestHandler<AddPartToWorkOrderCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public AddPartToWorkOrderCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(
        AddPartToWorkOrderCommand request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Los repuestos se cargan durante Diagnosing (armado del presupuesto) o durante
        // Approved/InProgress como ADICIONALES: nacen Pending, no suman al total ni van al
        // depósito hasta que el cliente los apruebe (DecideAdditionalItems).
        // En AwaitingApproval se bloquea: el presupuesto ya salió congelado — para modificarlo
        // hay que volver a Diagnosing vía "Modificar presupuesto" (ReviseQuote).
        if (workOrder.CurrentStatus is not (WorkOrderStatus.Diagnosing
                                         or WorkOrderStatus.Approved
                                         or WorkOrderStatus.InProgress))
            throw new BadRequestException(
                $"Solo se pueden agregar repuestos en 'Diagnosing' (presupuesto) o 'Approved'/'InProgress' (adicionales). Estado actual: '{workOrder.CurrentStatus}'.");

        var part = new WorkOrderPart
        {
            WorkOrderId    = workOrder.Id,
            ProductCode    = string.IsNullOrWhiteSpace(request.ProductCode) ? null : request.ProductCode.Trim(),
            Name           = request.Name.Trim(),
            UnitPrice      = request.UnitPrice,
            Quantity       = request.Quantity,
            ApprovalStatus = QuoteItemApprovalStatus.Pending,
        };

        workOrder.Parts.Add(part);
        workOrder.RecalculateTotalAmount();

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
