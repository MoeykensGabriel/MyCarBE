using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // ownership guard
using MyCarBE.Application.Features.VehicleTrips.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleTrips.Admin.Commands.CloseTripManually;

public class CloseTripManuallyCommandHandler : IRequestHandler<CloseTripManuallyCommand, VehicleTripDto>
{
    private readonly IVehicleTripRepository _tripRepository;
    private readonly IVehicleRepository     _vehicleRepository;
    private readonly ICurrentUserService    _currentUser;
    private readonly IUnitOfWork            _unitOfWork;
    private readonly IMapper                _mapper;

    public CloseTripManuallyCommandHandler(
        IVehicleTripRepository tripRepository,
        IVehicleRepository     vehicleRepository,
        ICurrentUserService    currentUser,
        IUnitOfWork            unitOfWork,
        IMapper                mapper)
    {
        _tripRepository    = tripRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
        _unitOfWork        = unitOfWork;
        _mapper            = mapper;
    }

    public async Task<VehicleTripDto> Handle(CloseTripManuallyCommand request, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetByIdAsync(request.TripId, cancellationToken)
            ?? throw new NotFoundException(nameof(VehicleTrip), request.TripId);

        // Ownership por vehículo
        var vehicle = await VehicleOwnershipGuard.EnsureAccessAsync(
            trip.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        if (trip.Status != VehicleTripStatus.Open)
            throw new BadRequestException("Solo se pueden cerrar viajes abiertos.");

        if (request.EndKm < trip.StartKm)
            throw new BadRequestException(
                $"El km de llegada ({request.EndKm}) no puede ser menor al de salida ({trip.StartKm}).");

        trip.EndKm   = request.EndKm;
        trip.EndedAt = DateTime.UtcNow;
        trip.Status  = VehicleTripStatus.ClosedByContact;

        if (request.EndKm > vehicle.CurrentMileage)
            vehicle.CurrentMileage = request.EndKm;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        trip.Vehicle = vehicle;
        return _mapper.Map<VehicleTripDto>(trip);
    }
}
