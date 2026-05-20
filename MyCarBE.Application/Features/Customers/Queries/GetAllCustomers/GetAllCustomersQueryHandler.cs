using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Customers.DTOs;

namespace MyCarBE.Application.Features.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper             _mapper;

    public GetAllCustomersQueryHandler(ICustomerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var paged = await _repository.SearchPagedAsync(request.SearchTerm, page, pageSize, cancellationToken);

        var items = _mapper.Map<IReadOnlyList<CustomerDto>>(paged.Items);
        return new PagedResult<CustomerDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
