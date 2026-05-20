using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, Guid>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IVehicleRepository   _vehicleRepository;
    private readonly ICurrentUserService  _currentUser;
    private readonly IUnitOfWork          _unitOfWork;

    public CreateWorkOrderCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IVehicleRepository   vehicleRepository,
        ICurrentUserService  currentUser,
        IUnitOfWork          unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _vehicleRepository   = vehicleRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
    }

    public async Task<Guid> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        // Load vehicle to snapshot ownership at entry time
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        var workOrder = new WorkOrder
        {
            VehicleId           = vehicle.Id,
            MileageAtEntry      = request.MileageAtEntry,
            CustomerNote        = string.IsNullOrWhiteSpace(request.CustomerNote)       ? null : request.CustomerNote.Trim(),
            ContactPersonName   = string.IsNullOrWhiteSpace(request.ContactPersonName)  ? null : request.ContactPersonName.Trim(),
            ContactPersonPhone  = string.IsNullOrWhiteSpace(request.ContactPersonPhone) ? null : request.ContactPersonPhone.Trim(),
            // Snapshot owner at the moment the work order is opened — frozen by EF config
            CustomerIdAtEntry   = vehicle.CustomerId,
            FleetIdAtEntry      = vehicle.FleetId,
            CreatedByUserId     = _currentUser.UserId,
        };

        // Initializes CurrentStatus = Received and records first StatusChange event
        workOrder.Initialize(_currentUser.UserId);

        await _workOrderRepository.AddAsync(workOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workOrder.Id;
    }
}
