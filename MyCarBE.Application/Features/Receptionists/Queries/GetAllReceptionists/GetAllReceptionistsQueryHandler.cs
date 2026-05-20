using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Receptionists.DTOs;

namespace MyCarBE.Application.Features.Receptionists.Queries.GetAllReceptionists;

public class GetAllReceptionistsQueryHandler : IRequestHandler<GetAllReceptionistsQuery, PagedResult<ReceptionistDto>>
{
    private readonly IReceptionistRepository _repository;
    private readonly IMapper                 _mapper;

    public GetAllReceptionistsQueryHandler(IReceptionistRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<PagedResult<ReceptionistDto>> Handle(GetAllReceptionistsQuery request, CancellationToken cancellationToken)
    {
        var page     = request.Page     <= 0 ? 1  : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var paged = await _repository.SearchPagedAsync(
            request.Search,
            request.IncludeInactive,
            page,
            pageSize,
            cancellationToken);

        var dtos = paged.Items.Select(r => _mapper.Map<ReceptionistDto>(r)).ToList();
        return new PagedResult<ReceptionistDto>(dtos, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
