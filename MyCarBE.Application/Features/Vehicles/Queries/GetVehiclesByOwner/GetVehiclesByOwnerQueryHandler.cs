using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Vehicles.DTOs;

namespace MyCarBE.Application.Features.Vehicles.Queries.GetVehiclesByOwner;

public class GetVehiclesByOwnerQueryHandler : IRequestHandler<GetVehiclesByOwnerQuery, IReadOnlyList<VehicleDto>>
{
    private readonly IVehicleRepository _repository;
    private readonly IMapper            _mapper;

    public GetVehiclesByOwnerQueryHandler(IVehicleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<IReadOnlyList<VehicleDto>> Handle(GetVehiclesByOwnerQuery request, CancellationToken cancellationToken)
    {
        if (request.CustomerId.HasValue)
        {
            var vehicles = await _repository.GetByCustomerIdAsync(request.CustomerId.Value, cancellationToken);
            return _mapper.Map<IReadOnlyList<VehicleDto>>(vehicles);
        }

        if (request.FleetId.HasValue)
        {
            var vehicles = await _repository.GetByFleetIdAsync(request.FleetId.Value, cancellationToken);
            return _mapper.Map<IReadOnlyList<VehicleDto>>(vehicles);
        }

        throw new BadRequestException("Debe proporcionar customerId o fleetId para filtrar los vehículos.");
    }
}
