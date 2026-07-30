using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.ClaimService;

public class ClaimServiceCommandHandler : IRequestHandler<ClaimServiceCommand>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ICurrentUserService  _currentUser;
    private readonly IUnitOfWork          _unitOfWork;

    public ClaimServiceCommandHandler(
        IWorkOrderRepository workOrderRepository,
        ICurrentUserService  currentUser,
        IUnitOfWork          unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
    }

    public async Task Handle(ClaimServiceCommand request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException(
                "Tu sesión no tiene perfil de ejecutante. Cerrá sesión y volvé a entrar para tomar trabajos.");

        var service = await _workOrderRepository.GetServiceByIdAsync(request.WorkOrderServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        // La WO debe estar en InProgress: el auto ya está físicamente en el taller.
        // En estados previos (Approved, AwaitingApproval, etc.) el cliente todavía no trajo el auto.
        if (service.WorkOrder.CurrentStatus != WorkOrderStatus.InProgress)
            throw new BadRequestException(
                $"Solo se pueden tomar trabajos cuando la orden está en progreso. " +
                $"Estado actual: {service.WorkOrder.CurrentStatus}.");

        // Las precondiciones de claim (Unassigned, Approved, sin mecánico) las valida el método de dominio.
        try
        {
            service.ClaimByMechanic(mechanicId);
        }
        catch (InvalidOperationException ex)
        {
            // Otro mecánico lo tomó entre el GET de la pantalla y el POST del claim
            // (race detectada antes de tocar la DB, vía estado en memoria).
            throw new ConflictException(ex.Message);
        }

        _workOrderRepository.Update(service.WorkOrder);

        // Si hay race real con otro mecánico, el xmin de Postgres hace fallar
        // SaveChangesAsync con DbUpdateConcurrencyException. El GlobalExceptionHandler
        // de la API la traduce a 409 Conflict. Mantenemos este handler libre de EF.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
