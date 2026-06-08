using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.InspectionReports.Commands.CreateInspectionReport;

public class CreateInspectionReportCommandHandler : IRequestHandler<CreateInspectionReportCommand, InspectionReportDto>
{
    private readonly IInspectionReportRepository _repository;
    private readonly IMechanicRepository         _mechanicRepository;
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly IVehicleTireRepository      _tireRepository;
    private readonly IVehicleBatteryRepository   _batteryRepository;
    private readonly ICurrentUserService         _currentUser;
    private readonly IUnitOfWork                 _unitOfWork;
    private readonly IMapper                     _mapper;

    public CreateInspectionReportCommandHandler(
        IInspectionReportRepository repository,
        IMechanicRepository         mechanicRepository,
        IWorkOrderRepository        workOrderRepository,
        IVehicleTireRepository      tireRepository,
        IVehicleBatteryRepository   batteryRepository,
        ICurrentUserService         currentUser,
        IUnitOfWork                 unitOfWork,
        IMapper                     mapper)
    {
        _repository          = repository;
        _mechanicRepository  = mechanicRepository;
        _workOrderRepository = workOrderRepository;
        _tireRepository      = tireRepository;
        _batteryRepository   = batteryRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<InspectionReportDto> Handle(CreateInspectionReportCommand request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException("Solo los mecánicos pueden crear reportes de inspección.");

        // Mecánico con sus áreas eager-loaded
        var mechanic = await _mechanicRepository.GetByIdWithAreasAsync(mechanicId, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), mechanicId);

        if (!mechanic.IsActive)
            throw new ForbiddenException("Mecánico desactivado no puede reportar inspecciones.");

        // Validar que el mecánico esté asignado al área del reporte
        var area = mechanic.Areas.FirstOrDefault(a => a.Id == request.AreaId)
            ?? throw new ForbiddenException("No estás asignado al área de este reporte.");

        // Las cubiertas solo las puede cargar el mecánico del área de cubiertas: es "el que sabe".
        var tires = request.Tires ?? Array.Empty<TireInspectionInput>();
        if (tires.Count > 0 && !area.IsTireArea)
            throw new BadRequestException(
                "Solo el reporte del área de cubiertas puede incluir datos de cubiertas.");

        // La batería solo la puede cargar el mecánico del área de batería.
        if (request.Battery is not null && !area.IsBatteryArea)
            throw new BadRequestException(
                "Solo el reporte del área de batería puede incluir datos de batería.");

        // Validar que la orden exista y esté en fase de inspección
        var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus != Domain.Enums.WorkOrderStatus.UnderInspection)
            throw new BadRequestException(
                $"Solo se puede reportar inspección en órdenes con estado UnderInspection. " +
                $"Estado actual: {workOrder.CurrentStatus}.");

        // Validar que no exista ya un reporte para esta (orden, área) — respeta el unique index a nivel app
        if (await _repository.ExistsForAreaAsync(request.WorkOrderId, request.AreaId, cancellationToken))
            throw new ConflictException(
                nameof(InspectionReport),
                "AreaId",
                $"Ya existe un reporte para esta área en la orden {request.WorkOrderId}.");

        var report = new InspectionReport
        {
            Id           = Guid.NewGuid(),
            WorkOrderId  = workOrder.Id,
            AreaId       = request.AreaId,
            MechanicId   = mechanic.Id,
            Findings     = string.IsNullOrWhiteSpace(request.Findings) ? null : request.Findings.Trim(),
            HasIssue     = request.HasIssue,
            IsNoFindings = false,
        };

        // Propuestas: solo si HasIssue=true tiene sentido. Si vienen con HasIssue=false las ignoramos.
        if (request.HasIssue)
        {
            if (request.ProposedServices is { Count: > 0 })
            {
                foreach (var ps in request.ProposedServices)
                {
                    report.ProposedServices.Add(new InspectionReportProposedService
                    {
                        Id                 = Guid.NewGuid(),
                        InspectionReportId = report.Id,
                        Name               = ps.Name.Trim(),
                        Description        = string.IsNullOrWhiteSpace(ps.Description) ? null : ps.Description.Trim(),
                        EstimatedLaborCost = ps.EstimatedLaborCost,
                        EstimatedDurationMinutes = ps.EstimatedDurationMinutes,
                    });
                }
            }

            if (request.ProposedParts is { Count: > 0 })
            {
                foreach (var pp in request.ProposedParts)
                {
                    report.ProposedParts.Add(new InspectionReportProposedPart
                    {
                        Id                 = Guid.NewGuid(),
                        InspectionReportId = report.Id,
                        Name               = pp.Name.Trim(),
                        Quantity           = pp.Quantity,
                        ProductCode        = string.IsNullOrWhiteSpace(pp.ProductCode) ? null : pp.ProductCode.Trim(),
                        EstimatedUnitPrice = pp.EstimatedUnitPrice,
                    });
                }
            }
        }

        await _repository.AddAsync(report, cancellationToken);

        // Datos de cubiertas cargados por el mecánico del área de cubiertas.
        // Para cada posición revisada: si no hay cubierta activa, la damos de alta usando los km
        // de ingreso de la orden como línea base; después registramos la medición de 3 puntos,
        // trazada a esta orden (WorkOrderId).
        if (tires.Count > 0)
        {
            var measuredOn = DateTime.UtcNow;
            var measuredByUserId = _currentUser.UserId == Guid.Empty ? (Guid?)null : _currentUser.UserId;

            foreach (var input in tires)
            {
                var tire = await _tireRepository.GetActiveByPositionAsync(
                    workOrder.VehicleId, input.Position, cancellationToken);

                if (tire is null)
                {
                    if (string.IsNullOrWhiteSpace(input.Brand) || string.IsNullOrWhiteSpace(input.SizeSpec))
                        throw new BadRequestException(
                            $"La posición {input.Position} no tiene cubierta registrada: " +
                            "indicá al menos marca y medida para darla de alta.");

                    tire = new VehicleTire
                    {
                        Id                  = Guid.NewGuid(),
                        VehicleId           = workOrder.VehicleId,
                        Position            = input.Position,
                        Brand               = input.Brand!.Trim(),
                        Model               = string.IsNullOrWhiteSpace(input.Model) ? string.Empty : input.Model!.Trim(),
                        SizeSpec            = input.SizeSpec!.Trim(),
                        InstalledOn         = DateOnly.FromDateTime(measuredOn),
                        InstalledAtKm       = workOrder.MileageAtEntry,
                        InitialTreadDepthMm = input.InitialTreadDepthMm ?? 8.0m,
                        ExpectedLifeKm      = input.ExpectedLifeKm ?? 0,
                        IsActive            = true,
                    };
                    await _tireRepository.AddAsync(tire, cancellationToken);
                }

                tire.Measurements.Add(new VehicleTireMeasurement
                {
                    Id                          = Guid.NewGuid(),
                    VehicleTireId               = tire.Id,
                    MeasuredOn                  = measuredOn,
                    VehicleMileageAtMeasurement = workOrder.MileageAtEntry,
                    InnerDepthMm                = input.InnerDepthMm,
                    CenterDepthMm               = input.CenterDepthMm,
                    OuterDepthMm                = input.OuterDepthMm,
                    Notes                       = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                    MeasuredByUserId            = measuredByUserId,
                    WorkOrderId                 = workOrder.Id,
                });
            }
        }

        // Estado de batería cargado por el mecánico del área de batería. Si el vehículo no
        // tiene batería registrada, la damos de alta con los km de ingreso como línea base;
        // después agregamos el chequeo trazado a esta orden.
        if (request.Battery is not null)
        {
            var checkedOn = DateTime.UtcNow;
            var checkedByUserId = _currentUser.UserId == Guid.Empty ? (Guid?)null : _currentUser.UserId;

            var battery = await _batteryRepository.GetActiveByVehicleAsync(workOrder.VehicleId, cancellationToken);

            if (battery is null)
            {
                battery = new VehicleBattery
                {
                    Id             = Guid.NewGuid(),
                    VehicleId      = workOrder.VehicleId,
                    Brand          = string.IsNullOrWhiteSpace(request.Battery.Brand) ? null : request.Battery.Brand.Trim(),
                    ManufacturedOn = request.Battery.ManufacturedOn,
                    InstalledOn    = DateOnly.FromDateTime(checkedOn),
                    InstalledAtKm  = workOrder.MileageAtEntry,
                    IsActive       = true,
                    CapacityAh           = request.Battery.CapacityAh,
                    BoxWidthCm           = request.Battery.BoxWidthCm,
                    BoxLengthCm          = request.Battery.BoxLengthCm,
                    BoxHeightCm          = request.Battery.BoxHeightCm,
                    PositiveTerminalSide = request.Battery.PositiveTerminalSide,
                };
                await _batteryRepository.AddAsync(battery, cancellationToken);
            }
            else
            {
                // Completar specs si no estaban cargados y ahora el mecánico los informa.
                if (!string.IsNullOrWhiteSpace(request.Battery.Brand) && string.IsNullOrWhiteSpace(battery.Brand))
                    battery.Brand = request.Battery.Brand.Trim();
                battery.ManufacturedOn       ??= request.Battery.ManufacturedOn;
                battery.CapacityAh           ??= request.Battery.CapacityAh;
                battery.BoxWidthCm           ??= request.Battery.BoxWidthCm;
                battery.BoxLengthCm          ??= request.Battery.BoxLengthCm;
                battery.BoxHeightCm          ??= request.Battery.BoxHeightCm;
                battery.PositiveTerminalSide ??= request.Battery.PositiveTerminalSide;
            }

            battery.Checks.Add(new VehicleBatteryCheck
            {
                Id                    = Guid.NewGuid(),
                VehicleBatteryId      = battery.Id,
                CheckedOn             = checkedOn,
                VehicleMileageAtCheck = workOrder.MileageAtEntry,
                Status                = request.Battery.Status,
                Voltage               = request.Battery.Voltage,
                RemainingPercentage   = request.Battery.RemainingPercentage,
                Notes                 = string.IsNullOrWhiteSpace(request.Battery.Notes) ? null : request.Battery.Notes.Trim(),
                CheckedByUserId       = checkedByUserId,
                WorkOrderId           = workOrder.Id,
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload con Area/Mechanic para el DTO
        var saved = await _repository.GetByWorkOrderAndAreaAsync(workOrder.Id, request.AreaId, cancellationToken);
        return _mapper.Map<InspectionReportDto>(saved!);
    }
}
