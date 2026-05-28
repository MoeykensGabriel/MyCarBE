using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleTrips.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleTrips.Public.Commands.EndTrip;

public class EndTripCommandHandler : IRequestHandler<EndTripCommand, VehicleTripDto>
{
    private readonly IVehicleTripRepository _tripRepository;
    private readonly IUnitOfWork            _unitOfWork;
    private readonly IMapper                _mapper;

    public EndTripCommandHandler(
        IVehicleTripRepository tripRepository,
        IUnitOfWork            unitOfWork,
        IMapper                mapper)
    {
        _tripRepository = tripRepository;
        _unitOfWork     = unitOfWork;
        _mapper         = mapper;
    }

    public async Task<VehicleTripDto> Handle(EndTripCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _tripRepository.GetVehicleByTripTokenAsync(request.Token, cancellationToken)
            ?? throw new NotFoundException("TripStation", request.Token);

        var openTrip = await _tripRepository.GetOpenTripForVehicleAsync(vehicle.Id, cancellationToken)
            ?? throw new BadRequestException("No hay un viaje abierto en este vehículo para cerrar.");

        if (request.EndKm < openTrip.StartKm)
            throw new BadRequestException(
                $"El km de llegada ({request.EndKm}) no puede ser menor al de salida ({openTrip.StartKm}).");

        openTrip.EndKm   = request.EndKm;
        openTrip.EndedAt = DateTime.UtcNow;
        openTrip.Status  = VehicleTripStatus.Closed;

        if (request.EndKm > vehicle.CurrentMileage)
            vehicle.CurrentMileage = request.EndKm;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        openTrip.Vehicle = vehicle;
        return _mapper.Map<VehicleTripDto>(openTrip);
    }
}
