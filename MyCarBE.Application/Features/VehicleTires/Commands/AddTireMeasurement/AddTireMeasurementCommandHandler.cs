using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // reusa VehicleOwnershipGuard
using MyCarBE.Application.Features.VehicleTires.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleTires.Commands.AddTireMeasurement;

public class AddTireMeasurementCommandHandler
    : IRequestHandler<AddTireMeasurementCommand, VehicleTireDto>
{
    private readonly IVehicleTireRepository _tireRepository;
    private readonly IVehicleRepository     _vehicleRepository;
    private readonly IWorkOrderRepository   _workOrderRepository;
    private readonly ICurrentUserService    _currentUser;
    private readonly IUnitOfWork            _unitOfWork;
    private readonly IMapper                _mapper;

    public AddTireMeasurementCommandHandler(
        IVehicleTireRepository tireRepository,
        IVehicleRepository     vehicleRepository,
        IWorkOrderRepository   workOrderRepository,
        ICurrentUserService    currentUser,
        IUnitOfWork            unitOfWork,
        IMapper                mapper)
    {
        _tireRepository      = tireRepository;
        _vehicleRepository   = vehicleRepository;
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<VehicleTireDto> Handle(AddTireMeasurementCommand request, CancellationToken cancellationToken)
    {
        var tire = await _tireRepository.GetByIdWithMeasurementsAsync(request.VehicleTireId, cancellationToken)
            ?? throw new NotFoundException(nameof(VehicleTire), request.VehicleTireId);

        // Que el usuario tenga acceso al vehículo dueño de esta cubierta.
        await VehicleOwnershipGuard.EnsureAccessAsync(
            tire.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        if (!tire.IsActive)
            throw new BadRequestException("No se pueden agregar mediciones a una cubierta reemplazada.");

        // Sanity check: medición no puede ser antes de que se instalara.
        if (request.VehicleMileageAtMeasurement < tire.InstalledAtKm)
            throw new BadRequestException(
                "Los km de la medición no pueden ser menores a los km de instalación de la cubierta.");

        // Si vino vinculada a una orden, validamos que exista y que sea del MISMO vehículo
        // (no tiene sentido trazar la medición a una visita de otro auto).
        if (request.WorkOrderId.HasValue)
        {
            var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), request.WorkOrderId.Value);

            if (workOrder.VehicleId != tire.VehicleId)
                throw new BadRequestException(
                    "La orden de trabajo indicada no corresponde al vehículo de esta cubierta.");
        }

        var measurement = new VehicleTireMeasurement
        {
            Id                          = Guid.NewGuid(),
            VehicleTireId               = tire.Id,
            MeasuredOn                  = request.MeasuredOn,
            VehicleMileageAtMeasurement = request.VehicleMileageAtMeasurement,
            InnerDepthMm                = request.InnerDepthMm,
            CenterDepthMm               = request.CenterDepthMm,
            OuterDepthMm                = request.OuterDepthMm,
            Notes                       = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            MeasuredByUserId            = _currentUser.UserId == Guid.Empty ? null : _currentUser.UserId,
            WorkOrderId                 = request.WorkOrderId,
        };

        tire.Measurements.Add(measurement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return VehicleTireDtoFactory.Build(tire, _mapper);
    }
}
