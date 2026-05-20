using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.UnassignMechanic;

public class UnassignMechanicCommandHandler : IRequestHandler<UnassignMechanicCommand>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;

    public UnassignMechanicCommandHandler(IWorkOrderRepository workOrderRepository, IUnitOfWork unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
    }

    public async Task Handle(UnassignMechanicCommand request, CancellationToken cancellationToken)
    {
        var service = await _workOrderRepository.GetServiceByIdAsync(request.WorkOrderServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        service.Unassign();

        _workOrderRepository.Update(service.WorkOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
