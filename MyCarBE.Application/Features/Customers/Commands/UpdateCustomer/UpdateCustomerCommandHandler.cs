using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Customers.DTOs;

namespace MyCarBE.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly IMapper             _mapper;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork         unitOfWork,
        IMapper             mapper)
    {
        _customerRepository = customerRepository;
        _unitOfWork         = unitOfWork;
        _mapper             = mapper;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Customer), request.Id);

        // Uniqueness checks — exclude this customer
        if (await _customerRepository.PhoneExistsAsync(request.Phone, request.Id, cancellationToken))
            throw new ConflictException(nameof(Domain.Entities.Customer), nameof(Domain.Entities.Customer.Phone), request.Phone);

        customer.FirstName = request.FirstName;
        customer.LastName  = request.LastName;
        customer.Phone     = request.Phone;
        customer.Email     = request.Email;
        customer.FleetId   = request.FleetId;   // null = unlink from fleet

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomerDto>(customer);
    }
}
