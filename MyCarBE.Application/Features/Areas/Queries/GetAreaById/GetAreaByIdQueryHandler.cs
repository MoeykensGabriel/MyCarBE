using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Areas.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Areas.Queries.GetAreaById;

public class GetAreaByIdQueryHandler : IRequestHandler<GetAreaByIdQuery, AreaDto>
{
    private readonly IAreaRepository _repository;
    private readonly IMapper         _mapper;

    public GetAreaByIdQueryHandler(IAreaRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<AreaDto> Handle(GetAreaByIdQuery request, CancellationToken cancellationToken)
    {
        var area = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Area), request.Id);

        return _mapper.Map<AreaDto>(area);
    }
}
