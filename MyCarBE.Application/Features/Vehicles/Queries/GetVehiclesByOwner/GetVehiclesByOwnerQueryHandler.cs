using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Vehicles.DTOs;

namespace MyCarBE.Application.Features.Vehicles.Queries.GetVehiclesByOwner;

public class GetVehiclesByOwnerQueryHandler : IRequestHandler<GetVehiclesByOwnerQuery, PagedResult<VehicleDto>>
{
    private readonly IVehicleRepository  _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper             _mapper;

    public GetVehiclesByOwnerQueryHandler(
        IVehicleRepository  repository,
        ICurrentUserService currentUser,
        IMapper             mapper)
    {
        _repository  = repository;
        _currentUser = currentUser;
        _mapper      = mapper;
    }

    public async Task<PagedResult<VehicleDto>> Handle(GetVehiclesByOwnerQuery request, CancellationToken cancellationToken)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        Guid? customerId;
        Guid? fleetId;

        if (_currentUser.IsAdmin || _currentUser.IsReceptionist)
        {
            // Admin y Receptionist respetan los filtros del query (búsqueda por patente, etc.)
            customerId = request.CustomerId;
            fleetId    = request.FleetId;
        }
        else
        {
            // Customer: ignora query params, usa sus propios IDs del JWT.
            // Si pertenece a una flota, sus vehículos son los de la flota
            // (los Vehicle de flota tienen CustomerId=null y FleetId=X, así que
            // no podemos pasar ambos al repo — se filtraría por customerId y daría 0).
            // Si es particular, filtra por su propio customerId.
            if (_currentUser.FleetId.HasValue)
            {
                customerId = null;
                fleetId    = _currentUser.FleetId;
            }
            else
            {
                customerId = _currentUser.CustomerId;
                fleetId    = null;
            }
        }

        var paged = await _repository.SearchPagedAsync(
            request.Search, customerId, fleetId, page, pageSize, cancellationToken);

        var items = _mapper.Map<IReadOnlyList<VehicleDto>>(paged.Items);
        return new PagedResult<VehicleDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
