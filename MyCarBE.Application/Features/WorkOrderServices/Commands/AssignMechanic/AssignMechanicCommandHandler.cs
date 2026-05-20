using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.AssignMechanic;

public class AssignMechanicCommandHandler : IRequestHandler<AssignMechanicCommand>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IMechanicRepository  _mechanicRepository;
    private readonly IUnitOfWork          _unitOfWork;

    public AssignMechanicCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IMechanicRepository  mechanicRepository,
        IUnitOfWork          unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _mechanicRepository  = mechanicRepository;
        _unitOfWork          = unitOfWork;
    }

    public async Task Handle(AssignMechanicCommand request, CancellationToken cancellationToken)
    {
        var service = await _workOrderRepository.GetServiceByIdAsync(request.WorkOrderServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        // No se puede asignar si la WorkOrder ya finalizó/canceló
        if (service.WorkOrder.CurrentStatus is WorkOrderStatus.Completed
                                            or WorkOrderStatus.Delivered
                                            or WorkOrderStatus.Cancelled)
            throw new BadRequestException(
                $"No se puede asignar mecánicos a una orden en estado '{service.WorkOrder.CurrentStatus}'.");

        var mechanic = await _mechanicRepository.GetByIdAsync(request.MechanicId, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), request.MechanicId);

        if (!mechanic.IsActive)
            throw new BadRequestException("No se puede asignar un mecánico inactivo.");

        service.AssignMechanic(mechanic.Id);

        _workOrderRepository.Update(service.WorkOrder); // toca UpdatedAt indirectamente
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
