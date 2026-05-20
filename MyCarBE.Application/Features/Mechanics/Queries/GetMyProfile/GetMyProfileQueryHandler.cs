using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMyProfile;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MechanicDto>
{
    private readonly IMechanicRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper             _mapper;

    public GetMyProfileQueryHandler(
        IMechanicRepository repository,
        ICurrentUserService currentUser,
        IMapper             mapper)
    {
        _repository  = repository;
        _currentUser = currentUser;
        _mapper      = mapper;
    }

    public async Task<MechanicDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var mechanicId = _currentUser.MechanicId
            ?? throw new ForbiddenException("Solo los mecánicos pueden acceder a este recurso.");

        var mechanic = await _repository.GetByIdAsync(mechanicId, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), mechanicId);

        return _mapper.Map<MechanicDto>(mechanic);
    }
}
