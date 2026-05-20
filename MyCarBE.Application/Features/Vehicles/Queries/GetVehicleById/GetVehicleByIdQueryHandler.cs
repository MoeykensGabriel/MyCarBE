using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Vehicles.DTOs;

namespace MyCarBE.Application.Features.Vehicles.Queries.GetVehicleById;

public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, VehicleDto>
{
    private readonly IVehicleRepository  _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper             _mapper;

    public GetVehicleByIdQueryHandler(
        IVehicleRepository  repository,
        ICurrentUserService currentUser,
        IMapper             mapper)
    {
        _repository  = repository;
        _currentUser = currentUser;
        _mapper      = mapper;
    }

    public async Task<VehicleDto> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Vehicle), request.Id);

        if (!_currentUser.IsAdmin)
        {
            var ownedByCustomer = _currentUser.CustomerId.HasValue &&
                                  vehicle.CustomerId == _currentUser.CustomerId;

            var ownedByFleet    = _currentUser.FleetId.HasValue &&
                                  vehicle.FleetId == _currentUser.FleetId;

            if (!ownedByCustomer && !ownedByFleet)
                throw new NotFoundException(nameof(Domain.Entities.Vehicle), request.Id);
        }

        return _mapper.Map<VehicleDto>(vehicle);
    }
}
