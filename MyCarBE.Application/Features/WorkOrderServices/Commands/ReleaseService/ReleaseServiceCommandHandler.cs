using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.ReleaseService;

public class ReleaseServiceCommandHandler : IRequestHandler<ReleaseServiceCommand>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ICurrentUserService  _currentUser;
    private readonly IUnitOfWork          _unitOfWork;

    public ReleaseServiceCommandHandler(
        IWorkOrderRepository workOrderRepository,
        ICurrentUserService  currentUser,
        IUnitOfWork          unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
    }

    public async Task Handle(ReleaseServiceCommand request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException(
                "Tu sesión no tiene perfil de ejecutante. Cerrá sesión y volvé a entrar para liberar trabajos.");

        var service = await _workOrderRepository.GetServiceByIdAsync(request.WorkOrderServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        // Ownership: 404 para el mecánico (no filtramos info), 403 explicativo para el admin.
        if (service.AssignedMechanicId != mechanicId)
        {
            if (_currentUser.IsAdmin)
                throw new ForbiddenException(
                    "Este trabajo está asignado a otro mecánico. Desasignalo desde la ficha de la orden.");

            throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);
        }

        // El método de dominio valida que el estado sea Pending (no Accepted/Completed).
        try
        {
            service.ReleaseByMechanic(mechanicId);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        _workOrderRepository.Update(service.WorkOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
