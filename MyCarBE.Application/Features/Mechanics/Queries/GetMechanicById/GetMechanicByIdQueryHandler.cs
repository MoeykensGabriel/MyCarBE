using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMechanicById;

public class GetMechanicByIdQueryHandler : IRequestHandler<GetMechanicByIdQuery, MechanicDto>
{
    private readonly IMechanicRepository _repository;
    private readonly IMapper             _mapper;

    public GetMechanicByIdQueryHandler(IMechanicRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<MechanicDto> Handle(GetMechanicByIdQuery request, CancellationToken cancellationToken)
    {
        var mechanic = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), request.Id);

        return _mapper.Map<MechanicDto>(mechanic);
    }
}
