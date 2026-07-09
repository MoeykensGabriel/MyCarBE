using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.CompleteServiceAsWorkshop;

public class CompleteServiceAsWorkshopCommandHandler : IRequestHandler<CompleteServiceAsWorkshopCommand>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ICurrentUserService  _currentUser;
    private readonly IUnitOfWork          _unitOfWork;

    public CompleteServiceAsWorkshopCommandHandler(
        IWorkOrderRepository workOrderRepository,
        ICurrentUserService  currentUser,
        IUnitOfWork          unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
    }

    public async Task Handle(CompleteServiceAsWorkshopCommand request, CancellationToken cancellationToken)
    {
        // Solo oficina — sanity check además del [Authorize] del controller.
        if (!_currentUser.IsAdmin && !_currentUser.IsReceptionist)
            throw new ForbiddenException("Solo admin u oficina pueden finalizar trabajos en nombre del taller.");

        var service = await _workOrderRepository.GetServiceByIdAsync(request.WorkOrderServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        if (service.WorkOrder.CurrentStatus != WorkOrderStatus.InProgress)
            throw new BadRequestException(
                $"La orden está en '{service.WorkOrder.CurrentStatus}'. Solo se pueden finalizar trabajos cuando la orden está en progreso.");

        try
        {
            service.CompleteByWorkshop(request.Notes);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
