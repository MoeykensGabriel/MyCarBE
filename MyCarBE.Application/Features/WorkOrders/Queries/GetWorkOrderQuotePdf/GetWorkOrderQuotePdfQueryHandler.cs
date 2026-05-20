using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderQuotePdf;

public class GetWorkOrderQuotePdfQueryHandler : IRequestHandler<GetWorkOrderQuotePdfQuery, byte[]>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IVehicleRepository   _vehicleRepository;
    private readonly ICustomerRepository  _customerRepository;
    private readonly IFleetRepository     _fleetRepository;
    private readonly IPdfService          _pdfService;
    private readonly ICurrentUserService  _currentUser;
    private readonly IMapper              _mapper;

    public GetWorkOrderQuotePdfQueryHandler(
        IWorkOrderRepository workOrderRepository,
        IVehicleRepository   vehicleRepository,
        ICustomerRepository  customerRepository,
        IFleetRepository     fleetRepository,
        IPdfService          pdfService,
        ICurrentUserService  currentUser,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _vehicleRepository   = vehicleRepository;
        _customerRepository  = customerRepository;
        _fleetRepository     = fleetRepository;
        _pdfService          = pdfService;
        _currentUser         = currentUser;
        _mapper              = mapper;
    }

    public async Task<byte[]> Handle(GetWorkOrderQuotePdfQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Solo permitimos descarga cuando ya existe un presupuesto formal.
        // En `Received` y `Diagnosing` aún no hay presupuesto que mostrar.
        if (workOrder.CurrentStatus is WorkOrderStatus.Received or WorkOrderStatus.Diagnosing)
            throw new BadRequestException("Todavía no hay un presupuesto disponible para esta orden.");

        // Ownership: si el usuario es Customer, solo puede descargar el PDF
        // de sus propias órdenes (directas o vía flota). Admin pasa siempre.
        if (!_currentUser.IsAdmin)
        {
            var customerId = _currentUser.CustomerId;
            var fleetId    = _currentUser.FleetId;

            var ownsAsCustomer = customerId.HasValue && workOrder.CustomerIdAtEntry == customerId;
            var ownsViaFleet   = fleetId.HasValue    && workOrder.FleetIdAtEntry    == fleetId;

            if (!ownsAsCustomer && !ownsViaFleet)
                throw new ForbiddenException("No tenés permiso para descargar este presupuesto.");
        }

        var vehicle = await _vehicleRepository.GetByIdAsync(workOrder.VehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), workOrder.VehicleId);

        string recipientName, recipientEmail;

        if (workOrder.CustomerIdAtEntry.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(workOrder.CustomerIdAtEntry.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Customer), workOrder.CustomerIdAtEntry.Value);
            recipientName  = $"{customer.FirstName} {customer.LastName}";
            recipientEmail = customer.Email ?? string.Empty;
        }
        else if (workOrder.FleetIdAtEntry.HasValue)
        {
            var fleet = await _fleetRepository.GetByIdAsync(workOrder.FleetIdAtEntry.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Fleet), workOrder.FleetIdAtEntry.Value);
            recipientName  = fleet.CompanyName;
            recipientEmail = fleet.Email ?? string.Empty;
        }
        else
        {
            throw new BadRequestException("La orden no tiene cliente ni flota asignada.");
        }

        var dto = _mapper.Map<WorkOrderDetailDto>(workOrder);

        var pdfData = new QuotePdfData(
            WorkOrder:      dto,
            LicensePlate:   vehicle.LicensePlate,
            VehicleBrand:   vehicle.Brand,
            VehicleModel:   vehicle.Model,
            VehicleYear:    vehicle.Year,
            RecipientName:  recipientName,
            RecipientEmail: recipientEmail);

        return _pdfService.GenerateQuotePdf(pdfData);
    }
}
