using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleTrips.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleTrips.Public.Commands.StartTrip;

public class StartTripCommandHandler : IRequestHandler<StartTripCommand, VehicleTripDto>
{
    private readonly IVehicleTripRepository _tripRepository;
    private readonly IUnitOfWork            _unitOfWork;
    private readonly IMapper                _mapper;

    public StartTripCommandHandler(
        IVehicleTripRepository tripRepository,
        IUnitOfWork            unitOfWork,
        IMapper                mapper)
    {
        _tripRepository = tripRepository;
        _unitOfWork     = unitOfWork;
        _mapper         = mapper;
    }

    public async Task<VehicleTripDto> Handle(StartTripCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _tripRepository.GetVehicleByTripTokenAsync(request.Token, cancellationToken)
            ?? throw new NotFoundException("TripStation", request.Token);

        // Auto-cierre del viaje anterior si quedó abierto
        var openTrip = await _tripRepository.GetOpenTripForVehicleAsync(vehicle.Id, cancellationToken);
        if (openTrip is not null)
        {
            openTrip.EndKm   = request.StartKm;
            openTrip.EndedAt = DateTime.UtcNow;
            openTrip.Status  = VehicleTripStatus.AutoClosed;
        }

        // Sanidad: km no puede ser absurdamente menor al ya conocido (permitimos 10 km de margen
        // por si el chofer corrigió un typo al pasar de uno a otro).
        var lastKm = openTrip?.StartKm ?? vehicle.CurrentMileage;
        if (request.StartKm < lastKm - 10)
            throw new BadRequestException(
                $"El km de salida ({request.StartKm}) es menor al último km conocido ({lastKm}). " +
                $"Verificá el número.");

        var trip = new VehicleTrip
        {
            Id             = Guid.NewGuid(),
            VehicleId      = vehicle.Id,
            DriverName     = request.DriverName.Trim(),
            DriverDocument = request.DriverDocument.Trim(),
            StartKm        = request.StartKm,
            StartedAt      = DateTime.UtcNow,
            Status         = VehicleTripStatus.Open,
        };

        await _tripRepository.AddAsync(trip, cancellationToken);

        // Sumamos también al currentMileage del vehículo para mantener consistencia
        if (request.StartKm > vehicle.CurrentMileage)
            vehicle.CurrentMileage = request.StartKm;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        trip.Vehicle = vehicle;
        return _mapper.Map<VehicleTripDto>(trip);
    }
}
