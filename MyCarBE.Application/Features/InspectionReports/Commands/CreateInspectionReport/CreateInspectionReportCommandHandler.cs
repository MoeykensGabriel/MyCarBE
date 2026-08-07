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
    private readonly IVehicleOilServiceRepository _oilRepository;
    private readonly IAreaRepository             _areaRepository;
    private readonly ICurrentUserService         _currentUser;
    private readonly IUnitOfWork                 _unitOfWork;
    private readonly IMapper                     _mapper;

    public CreateInspectionReportCommandHandler(
        IInspectionReportRepository repository,
        IMechanicRepository         mechanicRepository,
        IWorkOrderRepository        workOrderRepository,
        IVehicleTireRepository      tireRepository,
        IVehicleBatteryRepository   batteryRepository,
        IVehicleOilServiceRepository oilRepository,
        IAreaRepository             areaRepository,
        ICurrentUserService         currentUser,
        IUnitOfWork                 unitOfWork,
        IMapper                     mapper)
    {
        _repository          = repository;
        _mechanicRepository  = mechanicRepository;
        _workOrderRepository = workOrderRepository;
        _tireRepository      = tireRepository;
        _batteryRepository   = batteryRepository;
        _oilRepository       = oilRepository;
        _areaRepository      = areaRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<InspectionReportDto> Handle(CreateInspectionReportCommand request, CancellationToken cancellationToken)
    {
        // Quién reporta: el mecánico del área, o la oficina (admin/recepción) cuando
        // inspecciona ella misma — taller chico, o ningún mecánico del área disponible.
        // La oficina no tiene entidad Mechanic, así que el reporte queda con MechanicId
        // null: lo firma la oficina, igual que "sin novedades" y las áreas omitidas.
        var mechanicId = _currentUser.MechanicId;
        var isOffice   = _currentUser.IsAdmin || _currentUser.IsReceptionist;

        if (mechanicId is null && !isOffice)
            throw new ForbiddenException("Solo los mecánicos o la oficina pueden crear reportes de inspección.");

        // Mecánico con sus áreas eager-loaded (null cuando reporta el admin)
        Mechanic? mechanic = null;
        if (mechanicId is not null)
        {
            mechanic = await _mechanicRepository.GetByIdWithAreasAsync(mechanicId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Mechanic), mechanicId.Value);

            if (!mechanic.IsActive)
                throw new ForbiddenException("Mecánico desactivado no puede reportar inspecciones.");
        }

        // Validar que se pueda reportar el área:
        // - Admin o mecánico generalista: cualquier área activa (la resolvemos del catálogo).
        // - Mecánico normal: solo las áreas que tiene asignadas.
        Domain.Entities.Area area;
        if (mechanic is null || mechanic.IsGeneralist)
        {
            area = await _areaRepository.GetByIdAsync(request.AreaId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Entities.Area), request.AreaId);
            if (!area.IsActive)
                throw new BadRequestException("El área del reporte no está activa.");
        }
        else
        {
            area = mechanic.Areas.FirstOrDefault(a => a.Id == request.AreaId)
                ?? throw new ForbiddenException("No estás asignado al área de este reporte.");
        }

        // Las cubiertas solo las puede cargar el mecánico del área de cubiertas: es "el que sabe".
        var tires = request.Tires ?? Array.Empty<TireInspectionInput>();
        if (tires.Count > 0 && !area.IsTireArea)
            throw new BadRequestException(
                "Solo el reporte del área de cubiertas puede incluir datos de cubiertas.");

        // La batería solo la puede cargar el mecánico del área de batería.
        if (request.Battery is not null && !area.IsBatteryArea)
            throw new BadRequestException(
                "Solo el reporte del área de batería puede incluir datos de batería.");

        // El cambio de aceite solo lo puede cargar el mecánico del área de aceite.
        if (request.Oil is not null && !area.IsOilArea)
            throw new BadRequestException(
                "Solo el reporte del área de aceite puede incluir datos de cambio de aceite.");

        // Validar que la orden exista y esté en fase de inspección
        var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Dos canales de entrada:
        //   - Inspección inicial: la orden está en UnderInspection y se reporta cualquier área.
        //   - Inspección TARDÍA: la inspección ya cerró, pero un área quedó POSTERGADA y recién
        //     ahora se pudo mirar (se liberó el especialista, llegó el turno del elevador).
        //     Solo vale para áreas en deuda y mientras el hallazgo tenga cómo entrar al
        //     presupuesto — ver WorkOrder.AcceptsLateInspection.
        var isInitialInspection = workOrder.CurrentStatus == Domain.Enums.WorkOrderStatus.UnderInspection;

        if (!isInitialInspection)
        {
            if (!workOrder.AcceptsLateInspection)
                throw new BadRequestException(
                    $"No se puede reportar inspección con la orden en '{workOrder.CurrentStatus}'. " +
                    $"Estado actual: {workOrder.CurrentStatus}.");

            if (!await _repository.IsAreaPendingForVehicleAsync(workOrder.VehicleId, request.AreaId, cancellationToken))
                throw new BadRequestException(
                    "Con la inspección ya cerrada solo se pueden reportar las áreas que quedaron postergadas.");
        }

        // Un reporte vivo por (orden, área) — respeta el unique index parcial a nivel app.
        // En el canal tardío la postergación de ESTA orden es justamente lo que venimos a
        // reemplazar: la damos de baja (soft delete, igual que "Deshacer") para liberar el
        // índice, y queda como historial de que el área se había postergado.
        var existing = await _repository.GetByWorkOrderAndAreaAsync(request.WorkOrderId, request.AreaId, cancellationToken);
        if (existing is not null)
        {
            if (isInitialInspection || !existing.IsSkipped)
                throw new ConflictException(
                    nameof(InspectionReport),
                    "AreaId",
                    $"Ya existe un reporte para esta área en la orden {request.WorkOrderId}.");

            _repository.Delete(existing);
        }

        var report = new InspectionReport
        {
            Id           = Guid.NewGuid(),
            WorkOrderId  = workOrder.Id,
            AreaId       = request.AreaId,
            MechanicId   = mechanic?.Id,
            Findings     = string.IsNullOrWhiteSpace(request.Findings) ? null : request.Findings.Trim(),
            HasIssue     = request.HasIssue,
            IsNoFindings = false,
            // Marca de origen: entró por el canal tardío, con la inspección ya cerrada.
            IsLate       = !isInitialInspection,
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
                    // Fecha de instalación informada por el mecánico (de ahí se calcula la garantía).
                    // Si no la informa, cae a la fecha de la inspección.
                    InstalledOn    = request.Battery.InstalledOn ?? DateOnly.FromDateTime(checkedOn),
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

        // Cambio de aceite cargado por el mecánico del área de aceite (o un generalista).
        // Cada cambio es un evento nuevo: registra la línea base (km/fecha) e intervalos, de
        // donde sale la estimación del próximo service. Un nuevo registro reinicia los contadores.
        if (request.Oil is not null)
        {
            var changedByUserId = _currentUser.UserId == Guid.Empty ? (Guid?)null : _currentUser.UserId;

            await _oilRepository.AddAsync(new VehicleOilService
            {
                Id              = Guid.NewGuid(),
                VehicleId       = workOrder.VehicleId,
                ChangedOn       = request.Oil.ChangedOn ?? DateOnly.FromDateTime(workOrder.CreatedAt),
                // El km del cambio SIEMPRE es el del ingreso: el mecánico no puede poner
                // más ni menos km que los registrados al ingresar el vehículo. Ignoramos
                // cualquier ChangedAtKm que venga del cliente (regla de negocio).
                ChangedAtKm     = workOrder.MileageAtEntry,
                IntervalKm      = request.Oil.IntervalKm ?? 10000,
                IntervalMonths  = request.Oil.IntervalMonths ?? 6,
                OilType         = string.IsNullOrWhiteSpace(request.Oil.OilType) ? null : request.Oil.OilType.Trim(),
                OilBrand        = string.IsNullOrWhiteSpace(request.Oil.OilBrand) ? null : request.Oil.OilBrand.Trim(),
                FilterChanged   = request.Oil.FilterChanged ?? true,
                Notes           = string.IsNullOrWhiteSpace(request.Oil.Notes) ? null : request.Oil.Notes.Trim(),
                ChangedByUserId = changedByUserId,
                WorkOrderId     = workOrder.Id,
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload con Area/Mechanic para el DTO
        var saved = await _repository.GetByWorkOrderAndAreaAsync(workOrder.Id, request.AreaId, cancellationToken);
        return _mapper.Map<InspectionReportDto>(saved!);
    }
}
