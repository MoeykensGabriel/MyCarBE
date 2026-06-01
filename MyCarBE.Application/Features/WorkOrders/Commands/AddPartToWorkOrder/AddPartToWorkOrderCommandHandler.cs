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

        // Los repuestos se cargan durante Diagnosing (después de cerrar la inspección colectiva,
        // antes de enviar el presupuesto al cliente). En cualquier otro estado bloqueamos.
        if (workOrder.CurrentStatus != WorkOrderStatus.Diagnosing)
            throw new BadRequestException(
                $"Solo se pueden agregar repuestos a una orden en estado 'Diagnosing'. Estado actual: '{workOrder.CurrentStatus}'.");

        var part = new WorkOrderPart
        {
            WorkOrderId        = workOrder.Id,
            ProductCode        = string.IsNullOrWhiteSpace(request.ProductCode) ? null : request.ProductCode.Trim(),
            Name               = request.Name.Trim(),
            UnitPrice          = request.UnitPrice,
            CustomerUnitPrice  = request.CustomerUnitPrice,
            Quantity           = request.Quantity,
            Tier               = request.Tier,
            AlternativeGroupId = request.AlternativeGroupId,
            ApprovalStatus     = QuoteItemApprovalStatus.Pending,
        };

        workOrder.Parts.Add(part);
        workOrder.RecalculateTotalAmount();

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
