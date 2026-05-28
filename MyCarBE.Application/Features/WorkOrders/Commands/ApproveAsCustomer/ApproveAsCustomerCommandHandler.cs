using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.StockRequests.Services;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveAsCustomer;

public class ApproveAsCustomerCommandHandler : IRequestHandler<ApproveAsCustomerCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository       _workOrderRepository;
    private readonly ICurrentUserService        _currentUser;
    private readonly IUnitOfWork                _unitOfWork;
    private readonly IMapper                    _mapper;
    private readonly IStockRequestOrchestrator  _stockRequestOrchestrator;

    public ApproveAsCustomerCommandHandler(
        IWorkOrderRepository      workOrderRepository,
        ICurrentUserService       currentUser,
        IUnitOfWork               unitOfWork,
        IMapper                   mapper,
        IStockRequestOrchestrator stockRequestOrchestrator)
    {
        _workOrderRepository      = workOrderRepository;
        _currentUser              = currentUser;
        _unitOfWork               = unitOfWork;
        _mapper                   = mapper;
        _stockRequestOrchestrator = stockRequestOrchestrator;
    }

    public async Task<WorkOrderDetailDto> Handle(ApproveAsCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.CustomerId
            ?? throw new ForbiddenException("Solo los clientes pueden aprobar presupuestos desde su panel.");

        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), request.WorkOrderId);

        // Ownership: directo (customer) o vía flota.
        var fleetId = _currentUser.FleetId;
        var ownsAsCustomer = workOrder.CustomerIdAtEntry == customerId;
        var ownsViaFleet   = fleetId is not null && workOrder.FleetIdAtEntry == fleetId;

        if (!ownsAsCustomer && !ownsViaFleet)
            throw new ForbiddenException("No tenés permiso para aprobar esta orden.");

        try
        {
            workOrder.ApplyCustomerApproval(request.ApprovedServiceIds, request.ApprovedPartIds);

            workOrder.ChangeStatus(
                WorkOrderStatus.Approved,
                _currentUser.UserId,
                "Aprobado por el cliente desde su panel.");
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        // Crear el pedido al depósito antes del SaveChanges, así todo entra en una sola
        // transacción (la WO aprobada + el PartsStockRequest quedan consistentes).
        await _stockRequestOrchestrator.EnsureStockRequestForApprovedWorkOrderAsync(workOrder, cancellationToken);

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
