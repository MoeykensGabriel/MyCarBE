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
            ?? throw new ForbiddenException(
                "Tu sesión no tiene perfil de ejecutante. Cerrá sesión y volvé a entrar para aceptar trabajos.");

        var service = await _workOrderRepository.GetServiceByIdAsync(request.WorkOrderServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        // Ownership. Para el mecánico devolvemos 404 (leak prevention: no debe poder sondear
        // qué ids existen). Para el admin sería confuso — está mirando la fila en pantalla —
        // así que le decimos qué pasa y cuáles son sus salidas.
        if (service.AssignedMechanicId != mechanicId)
        {
            if (_currentUser.IsAdmin)
                throw new ForbiddenException(
                    "Este trabajo está asignado a otro mecánico. Reasignalo o finalizalo por taller.");

            throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);
        }

        // La WorkOrder debe estar InProgress para que se pueda trabajar
        if (service.WorkOrder.CurrentStatus != WorkOrderStatus.InProgress)
            throw new BadRequestException(
                $"La orden está en '{service.WorkOrder.CurrentStatus}'. Solo se pueden aceptar trabajos cuando la orden está en progreso.");

        try
        {
            service.AcceptByMechanic(mechanicId);
        }
        catch (InvalidOperationException ex)
        {
            // Doble click o dos pestañas sobre el mismo trabajo: sin esto sale 500,
            // porque el handler global no mapea InvalidOperationException.
            throw new ConflictException(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
