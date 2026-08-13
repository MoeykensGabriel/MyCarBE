using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Common.Security;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderClosingPdf;

public class GetWorkOrderClosingPdfQueryHandler
    : IRequestHandler<GetWorkOrderClosingPdfQuery, ClosingPdfResult>
{
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly IVehicleRepository          _vehicleRepository;
    private readonly ICustomerRepository         _customerRepository;
    private readonly IFleetRepository            _fleetRepository;
    private readonly IInspectionReportRepository _inspectionRepository;
    private readonly IOrderClosingPdfService     _pdfService;
    private readonly ICurrentUserService         _currentUser;
    private readonly IMapper                     _mapper;

    public GetWorkOrderClosingPdfQueryHandler(
        IWorkOrderRepository        workOrderRepository,
        IVehicleRepository          vehicleRepository,
        ICustomerRepository         customerRepository,
        IFleetRepository            fleetRepository,
        IInspectionReportRepository inspectionRepository,
        IOrderClosingPdfService     pdfService,
        ICurrentUserService         currentUser,
        IMapper                     mapper)
    {
        _workOrderRepository  = workOrderRepository;
        _vehicleRepository    = vehicleRepository;
        _customerRepository   = customerRepository;
        _fleetRepository      = fleetRepository;
        _inspectionRepository = inspectionRepository;
        _pdfService           = pdfService;
        _currentUser          = currentUser;
        _mapper               = mapper;
    }

    public async Task<ClosingPdfResult> Handle(
        GetWorkOrderClosingPdfQuery request,
        CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // El informe es de CIERRE: recién tiene sentido cuando la orden terminó. Antes sería
        // un documento incompleto que el cliente igual se guarda como si fuera definitivo.
        // Delivered también vale: la orden completada que ya se entregó es el caso normal de
        // "pasame el informe" un rato después.
        if (workOrder.CurrentStatus is not (WorkOrderStatus.Completed or WorkOrderStatus.Delivered))
            throw new BadRequestException(
                "El informe de cierre se genera cuando la orden está terminada. " +
                $"Estado actual: {workOrder.CurrentStatus}.");

        // La versión interna expone costos unitarios, códigos de proveedor y el nombre del
        // mecánico detrás de cada trabajo. Es material del taller: solo Admin.
        if (request.Internal && !_currentUser.IsAdmin)
            throw new ForbiddenException("El informe interno es solo para el administrador.");

        // Ownership: la oficina ve cualquier orden; el cliente solo las suyas (directas o
        // vía flota). Mismo criterio que el PDF del presupuesto.
        WorkOrderAccess.EnsureCanView(workOrder, _currentUser);

        var vehicle = await _vehicleRepository.GetByIdAsync(workOrder.VehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), workOrder.VehicleId);

        string  ownerName, ownerKind;
        string? ownerPhone, ownerEmail, ownerDocument;

        if (workOrder.CustomerIdAtEntry.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(workOrder.CustomerIdAtEntry.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Customer), workOrder.CustomerIdAtEntry.Value);

            ownerName     = $"{customer.FirstName} {customer.LastName}".Trim();
            ownerKind     = "Cliente";
            ownerPhone    = customer.Phone;
            ownerEmail    = customer.Email;
            ownerDocument = customer.DocumentNumber;
        }
        else if (workOrder.FleetIdAtEntry.HasValue)
        {
            var fleet = await _fleetRepository.GetByIdAsync(workOrder.FleetIdAtEntry.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Fleet), workOrder.FleetIdAtEntry.Value);

            ownerName     = fleet.CompanyName;
            ownerKind     = "Flota";
            ownerPhone    = fleet.Phone;
            ownerEmail    = fleet.Email;
            ownerDocument = fleet.TaxId;
        }
        else
        {
            // A diferencia del presupuesto, acá no cortamos: el informe describe lo que se le
            // hizo al vehículo y eso sigue siendo válido aunque el titular no esté cargado.
            ownerName     = "—";
            ownerKind     = "Titular";
            ownerPhone    = null;
            ownerEmail    = null;
            ownerDocument = null;
        }

        // Los reportes vienen del repositorio de inspecciones (no del detalle de la orden):
        // el DTO de la orden trae solo una vista liviana, y el informe necesita los hallazgos
        // completos con sus propuestas.
        var reports = await _inspectionRepository.GetByWorkOrderAsync(workOrder.Id, cancellationToken);
        var reportDtos = reports.Select(r => _mapper.Map<InspectionReportDto>(r)).ToList();

        var dto = _mapper.Map<WorkOrderDetailDto>(workOrder);

        var pdfData = new OrderClosingPdfData(
            WorkOrder:         dto,
            LicensePlate:      vehicle.LicensePlate,
            VehicleBrand:      vehicle.Brand,
            VehicleModel:      vehicle.Model,
            VehicleYear:       vehicle.Year,
            VehicleColor:      vehicle.Color,
            VehicleVin:        vehicle.VIN,
            OwnerName:         ownerName,
            OwnerKind:         ownerKind,
            OwnerPhone:        ownerPhone,
            OwnerEmail:        ownerEmail,
            OwnerDocument:     ownerDocument,
            InspectionReports: reportDtos,
            GeneratedAt:       DateTime.UtcNow,
            Internal:          request.Internal);

        return new ClosingPdfResult(_pdfService.GenerateClosingReport(pdfData), workOrder.Number);
    }
}
