using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderPart;

public class UpdateWorkOrderPartCommandHandler
    : IRequestHandler<UpdateWorkOrderPartCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public UpdateWorkOrderPartCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(
        UpdateWorkOrderPartCommand request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus != WorkOrderStatus.Diagnosing)
            throw new BadRequestException(
                $"Solo se pueden editar repuestos de una orden en estado 'Diagnosing'. Estado actual: '{workOrder.CurrentStatus}'.");

        var part = workOrder.Parts.FirstOrDefault(p => p.Id == request.PartId && !p.IsDeleted)
            ?? throw new NotFoundException(nameof(WorkOrderPart), request.PartId);

        if (part.FrozenAt.HasValue)
            throw new BadRequestException(
                "Este repuesto fue congelado al enviar el presupuesto y no se puede modificar.");

        part.ProductCode        = string.IsNullOrWhiteSpace(request.ProductCode) ? null : request.ProductCode.Trim();
        part.Name               = request.Name.Trim();
        part.UnitPrice          = request.UnitPrice;
        part.Quantity           = request.Quantity;
        part.Tier               = request.Tier;
        part.AlternativeGroupId = request.AlternativeGroupId;

        workOrder.RecalculateTotalAmount();

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
