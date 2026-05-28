using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.Application.Features.VehicleTrips.Public.Queries.GetTripStation;

public class GetTripStationQueryHandler : IRequestHandler<GetTripStationQuery, TripStationDto>
{
    private readonly IVehicleTripRepository _tripRepository;
    private readonly IMapper                _mapper;

    public GetTripStationQueryHandler(IVehicleTripRepository tripRepository, IMapper mapper)
    {
        _tripRepository = tripRepository;
        _mapper         = mapper;
    }

    public async Task<TripStationDto> Handle(GetTripStationQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await _tripRepository.GetVehicleByTripTokenAsync(request.Token, cancellationToken);
        if (vehicle is null)
            // 404 genérico — no aclaramos si el token existió alguna vez o no.
            throw new NotFoundException("TripStation", request.Token);

        var openTrip = await _tripRepository.GetOpenTripForVehicleAsync(vehicle.Id, cancellationToken);

        // Último km conocido: si hay open trip → su startKm; si no hay → currentMileage del vehículo
        // (o el endKm del último viaje cerrado, lo que sea mayor).
        int lastKm = vehicle.CurrentMileage;
        var lastClosed = await _tripRepository.GetLastClosedTripForVehicleAsync(vehicle.Id, cancellationToken);
        if (lastClosed?.EndKm is int closedEnd && closedEnd > lastKm) lastKm = closedEnd;
        if (openTrip?.StartKm is int openStart && openStart > lastKm) lastKm = openStart;

        // Para mapear el open trip necesitamos cargar Vehicle (ya lo tenemos arriba).
        VehicleTripDto? openDto = null;
        if (openTrip is not null)
        {
            openTrip.Vehicle = vehicle; // evita un segundo round-trip
            openDto = _mapper.Map<VehicleTripDto>(openTrip);
        }

        return new TripStationDto(
            VehicleId:   vehicle.Id,
            LicensePlate: vehicle.LicensePlate,
            Brand:        vehicle.Brand,
            Model:        vehicle.Model,
            LastKnownKm:  lastKm,
            OpenTrip:     openDto
        );
    }
}
