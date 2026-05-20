using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveAsCustomer;

public class ApproveAsCustomerCommandHandler : IRequestHandler<ApproveAsCustomerCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ICurrentUserService  _currentUser;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public ApproveAsCustomerCommandHandler(
        IWorkOrderRepository workOrderRepository,
        ICurrentUserService  currentUser,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(ApproveAsCustomerCommand request, CancellationToken cancellationToken)
    {
        // El usuario debe ser un Customer logueado (con CustomerId asociado).
        var customerId = _currentUser.CustomerId
            ?? throw new ForbiddenException("Solo los clientes pueden aprobar presupuestos desde su panel.");

        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), request.WorkOrderId);

        // Ownership: el cliente puede aprobar si la WO le pertenece directamente
        // (CustomerIdAtEntry) o si pertenece a su flota (FleetIdAtEntry).
        var fleetId = _currentUser.FleetId;
        var ownsAsCustomer = workOrder.CustomerIdAtEntry == customerId;
        var ownsViaFleet   = fleetId is not null && workOrder.FleetIdAtEntry == fleetId;

        if (!ownsAsCustomer && !ownsViaFleet)
            throw new ForbiddenException("No tenés permiso para aprobar esta orden.");

        if (workOrder.CurrentStatus != WorkOrderStatus.AwaitingApproval)
            throw new BadRequestException(
                $"La orden no está pendiente de aprobación. Estado actual: {workOrder.CurrentStatus}.");

        try
        {
            // El cliente aprueba → pasa a Approved (todavía no In Progress).
            // El admin debe pasar manualmente a InProgress cuando el vehículo esté en el taller.
            workOrder.ChangeStatus(
                WorkOrderStatus.Approved,
                _currentUser.UserId,
                "Aprobado por el cliente desde su panel.");
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
