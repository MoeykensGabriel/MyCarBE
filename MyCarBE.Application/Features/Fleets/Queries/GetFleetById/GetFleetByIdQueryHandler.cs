using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Fleets.DTOs;

namespace MyCarBE.Application.Features.Fleets.Queries.GetFleetById;

public class GetFleetByIdQueryHandler : IRequestHandler<GetFleetByIdQuery, FleetDetailDto>
{
    private readonly IFleetRepository _repository;
    private readonly IMapper          _mapper;

    public GetFleetByIdQueryHandler(IFleetRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<FleetDetailDto> Handle(GetFleetByIdQuery request, CancellationToken cancellationToken)
    {
        var fleet = await _repository.GetWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Fleet), request.Id);

        return _mapper.Map<FleetDetailDto>(fleet);
    }
}
