using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.CompleteService;

public class CompleteServiceCommandHandler : IRequestHandler<CompleteServiceCommand>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ICurrentUserService  _currentUser;
    private readonly IUnitOfWork          _unitOfWork;

    public CompleteServiceCommandHandler(
        IWorkOrderRepository workOrderRepository,
        ICurrentUserService  currentUser,
        IUnitOfWork          unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
    }

    public async Task Handle(CompleteServiceCommand request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException("Solo los mecánicos pueden finalizar trabajos.");

        var service = await _workOrderRepository.GetServiceByIdAsync(request.WorkOrderServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        // Ownership: si el servicio no le pertenece, 404 (leak prevention)
        if (service.AssignedMechanicId != mechanicId)
            throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        if (service.WorkOrder.CurrentStatus != WorkOrderStatus.InProgress)
            throw new BadRequestException(
                $"La orden está en '{service.WorkOrder.CurrentStatus}'. Solo se pueden finalizar trabajos cuando la orden está en progreso.");

        service.CompleteByMechanic(mechanicId, request.Notes, request.Findings);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
