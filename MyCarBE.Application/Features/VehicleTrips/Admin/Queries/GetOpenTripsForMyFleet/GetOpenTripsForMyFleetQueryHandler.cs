using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.Application.Features.VehicleTrips.Admin.Queries.GetOpenTripsForMyFleet;

public class GetOpenTripsForMyFleetQueryHandler
    : IRequestHandler<GetOpenTripsForMyFleetQuery, IReadOnlyList<VehicleTripDto>>
{
    private readonly IVehicleTripRepository _tripRepository;
    private readonly ICurrentUserService    _currentUser;
    private readonly IMapper                _mapper;

    public GetOpenTripsForMyFleetQueryHandler(
        IVehicleTripRepository tripRepository,
        ICurrentUserService    currentUser,
        IMapper                mapper)
    {
        _tripRepository = tripRepository;
        _currentUser    = currentUser;
        _mapper         = mapper;
    }

    public async Task<IReadOnlyList<VehicleTripDto>> Handle(GetOpenTripsForMyFleetQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.FleetId.HasValue)
            throw new ForbiddenException("Solo encargados de flota pueden ver esto.");

        var trips = await _tripRepository.GetOpenTripsByFleetAsync(_currentUser.FleetId.Value, cancellationToken);
        return trips.Select(t => _mapper.Map<VehicleTripDto>(t)).ToList();
    }
}
