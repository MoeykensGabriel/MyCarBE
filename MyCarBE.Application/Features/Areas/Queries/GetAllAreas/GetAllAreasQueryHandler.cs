using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Areas.DTOs;

namespace MyCarBE.Application.Features.Areas.Queries.GetAllAreas;

public class GetAllAreasQueryHandler : IRequestHandler<GetAllAreasQuery, IReadOnlyList<AreaDto>>
{
    private readonly IAreaRepository _repository;
    private readonly IMapper         _mapper;

    public GetAllAreasQueryHandler(IAreaRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<IReadOnlyList<AreaDto>> Handle(GetAllAreasQuery request, CancellationToken cancellationToken)
    {
        var areas = await _repository.GetAllAsync(request.IncludeInactive, cancellationToken);
        return areas.Select(a => _mapper.Map<AreaDto>(a)).ToList();
    }
}
