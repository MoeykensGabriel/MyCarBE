using System.Security.Cryptography;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // reusamos el ownership guard

namespace MyCarBE.Application.Features.VehicleTrips.Admin.Commands.RegenerateTripToken;

public class RegenerateTripTokenCommandHandler : IRequestHandler<RegenerateTripTokenCommand, string>
{
    private readonly IVehicleRepository     _vehicleRepository;
    private readonly IVehicleTripRepository _tripRepository;
    private readonly ICurrentUserService    _currentUser;
    private readonly IUnitOfWork            _unitOfWork;

    public RegenerateTripTokenCommandHandler(
        IVehicleRepository     vehicleRepository,
        IVehicleTripRepository tripRepository,
        ICurrentUserService    currentUser,
        IUnitOfWork            unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _tripRepository    = tripRepository;
        _currentUser       = currentUser;
        _unitOfWork        = unitOfWork;
    }

    public async Task<string> Handle(RegenerateTripTokenCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        // Solo flotas tienen sentido para esto — los autos de cliente particular no tienen
        // "estación de viajes". Si en el futuro se quiere, se relaja.
        if (vehicle.FleetId is null)
            throw new BadRequestException(
                "La estación de viajes (QR) solo aplica a vehículos de flota.");

        // Generar token único, reintentando ante colisión (chance ínfima con 32 bytes).
        string token;
        do
        {
            var bytes = RandomNumberGenerator.GetBytes(24);
            token = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        } while (await _tripRepository.TripTokenExistsAsync(token, cancellationToken));

        vehicle.TripToken = token;
        _vehicleRepository.Update(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return token;
    }
}
