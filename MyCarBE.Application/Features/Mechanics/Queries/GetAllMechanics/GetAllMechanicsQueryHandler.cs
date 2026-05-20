using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetAllMechanics;

public class GetAllMechanicsQueryHandler : IRequestHandler<GetAllMechanicsQuery, PagedResult<MechanicDto>>
{
    private readonly IMechanicRepository _repository;
    private readonly IMapper             _mapper;

    public GetAllMechanicsQueryHandler(IMechanicRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<PagedResult<MechanicDto>> Handle(GetAllMechanicsQuery request, CancellationToken cancellationToken)
    {
        var page     = request.Page     <= 0 ? 1  : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var paged = await _repository.SearchPagedAsync(
            request.Search,
            request.IncludeInactive,
            page,
            pageSize,
            cancellationToken);

        var dtos = paged.Items.Select(m => _mapper.Map<MechanicDto>(m)).ToList();
        return new PagedResult<MechanicDto>(dtos, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
