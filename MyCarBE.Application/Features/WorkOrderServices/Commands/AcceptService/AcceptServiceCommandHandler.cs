using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.AcceptService;

public class AcceptServiceCommandHandler : IRequestHandler<AcceptServiceCommand>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ICurrentUserService  _currentUser;
    private readonly IUnitOfWork          _unitOfWork;

    public AcceptServiceCommandHandler(
        IWorkOrderRepository workOrderRepository,
        ICurrentUserService  currentUser,
        IUnitOfWork          unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
    }

    public async Task Handle(AcceptServiceCommand request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException("Solo los mecánicos pueden aceptar trabajos.");

        var service = await _workOrderRepository.GetServiceByIdAsync(request.WorkOrderServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        // Ownership: si el servicio no le pertenece, devolvemos 404 (leak prevention)
        if (service.AssignedMechanicId != mechanicId)
            throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        // La WorkOrder debe estar InProgress para que se pueda trabajar
        if (service.WorkOrder.CurrentStatus != WorkOrderStatus.InProgress)
            throw new BadRequestException(
                $"La orden está en '{service.WorkOrder.CurrentStatus}'. Solo se pueden aceptar trabajos cuando la orden está en progreso.");

        service.AcceptByMechanic(mechanicId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
