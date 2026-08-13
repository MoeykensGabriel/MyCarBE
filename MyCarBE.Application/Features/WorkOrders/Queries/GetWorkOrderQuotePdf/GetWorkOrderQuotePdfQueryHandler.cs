using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Common.Security;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderQuotePdf;

public class GetWorkOrderQuotePdfQueryHandler : IRequestHandler<GetWorkOrderQuotePdfQuery, QuotePdfResult>
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

    public async Task<QuotePdfResult> Handle(GetWorkOrderQuotePdfQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Solo permitimos descarga cuando ya existe un presupuesto formal.
        // En `Received` y `Diagnosing` aún no hay presupuesto que mostrar.
        if (workOrder.CurrentStatus is WorkOrderStatus.Received or WorkOrderStatus.Diagnosing)
            throw new BadRequestException("Todavía no hay un presupuesto disponible para esta orden.");

        // Una orden de solo inspección nunca tiene presupuesto, pero sí llega a Completed,
        // así que pasaría el check de arriba y generaría un PDF con cero items.
        if (workOrder.IsInspectionOnly)
            throw new BadRequestException(
                "Esta orden es de solo inspección: no tiene presupuesto. El resultado de la " +
                "inspección se consulta en el detalle de la orden.");

        // La oficina descarga el presupuesto de cualquier orden; el cliente, el de las suyas.
        WorkOrderAccess.EnsureCanView(workOrder, _currentUser);

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

        return new QuotePdfResult(_pdfService.GenerateQuotePdf(pdfData), workOrder.Number);
    }
}
