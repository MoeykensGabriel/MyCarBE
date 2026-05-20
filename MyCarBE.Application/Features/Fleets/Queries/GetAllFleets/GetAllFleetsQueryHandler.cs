using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Fleets.DTOs;

namespace MyCarBE.Application.Features.Fleets.Queries.GetAllFleets;

public class GetAllFleetsQueryHandler : IRequestHandler<GetAllFleetsQuery, IReadOnlyList<FleetDto>>
{
    private readonly IFleetRepository _repository;
    private readonly IMapper          _mapper;

    public GetAllFleetsQueryHandler(IFleetRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<IReadOnlyList<FleetDto>> Handle(GetAllFleetsQuery request, CancellationToken cancellationToken)
    {
        var fleets = await _repository.SearchAsync(request.SearchTerm, cancellationToken);
        return _mapper.Map<IReadOnlyList<FleetDto>>(fleets);
    }
}
