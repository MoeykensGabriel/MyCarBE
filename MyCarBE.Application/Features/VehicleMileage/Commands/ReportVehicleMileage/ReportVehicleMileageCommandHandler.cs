using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Formatting;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // VehicleOwnershipGuard (compartido)
using MyCarBE.Application.Features.VehicleMileage.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleMileage.Commands.ReportVehicleMileage;

public class ReportVehicleMileageCommandHandler
    : IRequestHandler<ReportVehicleMileageCommand, VehicleMileageReadingDto>
{
    /// <summary>
    /// Tope de plausibilidad: más de este promedio diario desde la última lectura
    /// es casi seguro un typo (un dígito de más). 1.000 km/día ya es un uso extremo.
    /// </summary>
    private const int MaxPlausibleKmPerDay = 1_000;

    private readonly IVehicleRepository  _vehicleRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _unitOfWork;

    public ReportVehicleMileageCommandHandler(
        IVehicleRepository  vehicleRepository,
        ICurrentUserService currentUser,
        IUnitOfWork         unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
        _unitOfWork        = unitOfWork;
    }

    public async Task<VehicleMileageReadingDto> Handle(
        ReportVehicleMileageCommand request, CancellationToken cancellationToken)
    {
        // Admin / Customer dueño / contacto de la flota. Otro rol → 404 (anti-enumeración).
        var vehicle = await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        // Monotonía: el odómetro no retrocede. Igual está permitido (= "no lo usé",
        // que legítimamente renueva el recordatorio).
        if (request.Mileage < vehicle.CurrentMileage)
            throw new BadRequestException(
                $"El kilometraje no puede ser menor a la última lectura " +
                $"({MoneyFormat.ArNumber(vehicle.CurrentMileage)} km). Si la lectura anterior está mal, contactá al taller.");

        // Plausibilidad: salto absurdo desde la última lectura → typo casi seguro.
        // Solo el admin puede pasar por encima (corrige errores reales).
        if (!_currentUser.IsAdmin && vehicle.MileageUpdatedAt is { } lastAt)
        {
            var days  = Math.Max(1, (DateTime.UtcNow - lastAt).TotalDays);
            var delta = request.Mileage - vehicle.CurrentMileage;
            if (delta / days > MaxPlausibleKmPerDay)
                throw new BadRequestException(
                    $"El valor implica más de {MoneyFormat.ArNumber(MaxPlausibleKmPerDay)} km por día desde la última " +
                    $"lectura ({MoneyFormat.ArNumber(vehicle.CurrentMileage)} km). Revisá que no haya un dígito de más; " +
                    "si el valor es correcto, contactá al taller.");
        }

        var source = _currentUser.IsAdmin
            ? MileageReadingSource.AdminAdjustment
            : MileageReadingSource.CustomerReport;
        var userId = _currentUser.UserId == Guid.Empty ? (Guid?)null : _currentUser.UserId;

        var reading = vehicle.AddMileageReading(request.Mileage, source, userId);
        _vehicleRepository.Update(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VehicleMileageReadingDto(reading.Id, reading.Mileage, reading.Source, reading.CreatedAt);
    }
}
