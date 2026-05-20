using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Validation;
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

        var normalizedPhone = ArgentinaIdentifiers.NormalizePhone(request.Phone);

        // Uniqueness checks — exclude this customer
        if (await _customerRepository.PhoneExistsAsync(normalizedPhone, request.Id, cancellationToken))
            throw new ConflictException(nameof(Domain.Entities.Customer), nameof(Domain.Entities.Customer.Phone), normalizedPhone);

        customer.FirstName = request.FirstName.Trim();
        customer.LastName  = request.LastName.Trim();
        customer.Phone     = normalizedPhone;
        customer.Email     = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        customer.FleetId   = request.FleetId;   // null = unlink from fleet

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomerDto>(customer);
    }
}
