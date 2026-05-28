using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // reusa VehicleOwnershipGuard
using MyCarBE.Application.Features.VehicleTires.DTOs;

namespace MyCarBE.Application.Features.VehicleTires.Queries.GetTiresByVehicle;

public class GetTiresByVehicleQueryHandler
    : IRequestHandler<GetTiresByVehicleQuery, IReadOnlyList<VehicleTireDto>>
{
    private readonly IVehicleTireRepository _tireRepository;
    private readonly IVehicleRepository     _vehicleRepository;
    private readonly ICurrentUserService    _currentUser;
    private readonly IMapper                _mapper;

    public GetTiresByVehicleQueryHandler(
        IVehicleTireRepository tireRepository,
        IVehicleRepository     vehicleRepository,
        ICurrentUserService    currentUser,
        IMapper                mapper)
    {
        _tireRepository    = tireRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
        _mapper            = mapper;
    }

    public async Task<IReadOnlyList<VehicleTireDto>> Handle(GetTiresByVehicleQuery request, CancellationToken cancellationToken)
    {
        // Guard devuelve el Vehicle ya cargado; aprovechamos el CurrentMileage para
        // anclar la estimación al kilometraje vigente del auto.
        var vehicle = await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        var tires = await _tireRepository.GetByVehicleAsync(
            request.VehicleId, request.IncludeReplaced, cancellationToken);

        return tires
            .Select(t => VehicleTireDtoFactory.Build(t, _mapper, vehicle.CurrentMileage))
            .ToList();
    }
}
