using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Customers.DTOs;

namespace MyCarBE.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper             _mapper;

    public GetCustomerByIdQueryHandler(ICustomerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Customer), request.Id);

        return _mapper.Map<CustomerDto>(customer);
    }
}
