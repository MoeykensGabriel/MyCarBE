using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Maintenance.Commands.ResetMaintenanceAlert;

public class ResetMaintenanceAlertCommandHandler
    : IRequestHandler<ResetMaintenanceAlertCommand, MaintenanceAlertConfigDto>
{
    private readonly IVehicleRepository          _vehicleRepository;
    private readonly IMaintenanceAlertRepository _alertRepository;
    private readonly IUnitOfWork                 _unitOfWork;

    public ResetMaintenanceAlertCommandHandler(
        IVehicleRepository          vehicleRepository,
        IMaintenanceAlertRepository alertRepository,
        IUnitOfWork                 unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _alertRepository   = alertRepository;
        _unitOfWork        = unitOfWork;
    }

    public async Task<MaintenanceAlertConfigDto> Handle(
        ResetMaintenanceAlertCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        var alerts = await _alertRepository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        var alert  = alerts.FirstOrDefault(a => a.Id == request.AlertId)
            ?? throw new NotFoundException(nameof(MaintenanceAlert), request.AlertId);

        var now = DateTime.UtcNow;
        alert.ResetCycle(vehicle.CurrentMileage, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return alert.ToConfigDto(vehicle.CurrentMileage, now);
    }
}
