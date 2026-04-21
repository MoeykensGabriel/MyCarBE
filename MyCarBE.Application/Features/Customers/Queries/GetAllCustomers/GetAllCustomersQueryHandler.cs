using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Customers.DTOs;

namespace MyCarBE.Application.Features.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper             _mapper;

    public GetAllCustomersQueryHandler(ICustomerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<IReadOnlyList<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _repository.SearchAsync(request.SearchTerm, cancellationToken);
        return _mapper.Map<IReadOnlyList<CustomerDto>>(customers);
    }
}
