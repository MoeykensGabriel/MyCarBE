using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Customers.DTOs;

namespace MyCarBE.Application.Features.Customers.Queries.GetCurrentCustomer;

public class GetCurrentCustomerQueryHandler : IRequestHandler<GetCurrentCustomerQuery, CustomerDto>
{
    private readonly ICustomerRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper             _mapper;

    public GetCurrentCustomerQueryHandler(
        ICustomerRepository repository,
        ICurrentUserService currentUser,
        IMapper             mapper)
    {
        _repository  = repository;
        _currentUser = currentUser;
        _mapper      = mapper;
    }

    public async Task<CustomerDto> Handle(GetCurrentCustomerQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CustomerId.HasValue)
            throw new UnauthorizedAccessException("Este endpoint es solo para usuarios Customer.");

        var customer = await _repository.GetByIdAsync(_currentUser.CustomerId.Value, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Customer), _currentUser.CustomerId.Value);

        return _mapper.Map<CustomerDto>(customer);
    }
}
