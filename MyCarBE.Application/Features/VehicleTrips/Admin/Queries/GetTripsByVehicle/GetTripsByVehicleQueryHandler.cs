using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments;
using MyCarBE.Application.Features.VehicleTrips.DTOs;

namespace MyCarBE.Application.Features.VehicleTrips.Admin.Queries.GetTripsByVehicle;

public class GetTripsByVehicleQueryHandler
    : IRequestHandler<GetTripsByVehicleQuery, IReadOnlyList<VehicleTripDto>>
{
    private readonly IVehicleTripRepository _tripRepository;
    private readonly IVehicleRepository     _vehicleRepository;
    private readonly ICurrentUserService    _currentUser;
    private readonly IMapper                _mapper;

    public GetTripsByVehicleQueryHandler(
        IVehicleTripRepository tripRepository,
        IVehicleRepository     vehicleRepository,
        ICurrentUserService    currentUser,
        IMapper                mapper)
    {
        _tripRepository    = tripRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
        _mapper            = mapper;
    }

    public async Task<IReadOnlyList<VehicleTripDto>> Handle(GetTripsByVehicleQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        var trips = await _tripRepository.GetTripsByVehicleAsync(vehicle.Id, cancellationToken);
        // El repo no incluye Vehicle pero ya lo tenemos para mapear
        foreach (var t in trips) t.Vehicle = vehicle;

        return trips.Select(t => _mapper.Map<VehicleTripDto>(t)).ToList();
    }
}
