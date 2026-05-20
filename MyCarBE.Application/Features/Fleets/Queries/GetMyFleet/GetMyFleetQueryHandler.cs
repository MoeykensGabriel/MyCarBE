using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Fleets.DTOs;

namespace MyCarBE.Application.Features.Fleets.Queries.GetMyFleet;

public class GetMyFleetQueryHandler : IRequestHandler<GetMyFleetQuery, FleetDetailDto>
{
    private readonly IFleetRepository    _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper             _mapper;

    public GetMyFleetQueryHandler(
        IFleetRepository    repository,
        ICurrentUserService currentUser,
        IMapper             mapper)
    {
        _repository  = repository;
        _currentUser = currentUser;
        _mapper      = mapper;
    }

    public async Task<FleetDetailDto> Handle(GetMyFleetQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.FleetId.HasValue)
            throw new UnauthorizedAccessException("Este endpoint es solo para usuarios vinculados a una flota.");

        var fleet = await _repository.GetWithDetailsAsync(_currentUser.FleetId.Value, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Fleet), _currentUser.FleetId.Value);

        return _mapper.Map<FleetDetailDto>(fleet);
    }
}
