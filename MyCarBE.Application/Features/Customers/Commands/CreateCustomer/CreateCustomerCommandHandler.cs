using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Customers.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResponseDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IIdentityService    _identityService;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly IMapper             _mapper;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IIdentityService    identityService,
        IUnitOfWork         unitOfWork,
        IMapper             mapper)
    {
        _customerRepository = customerRepository;
        _identityService    = identityService;
        _unitOfWork         = unitOfWork;
        _mapper             = mapper;
    }

    public async Task<CreateCustomerResponseDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // 1. Uniqueness checks
        if (await _customerRepository.DocumentNumberExistsAsync(request.DocumentNumber, cancellationToken))
            throw new ConflictException(nameof(Customer), nameof(Customer.DocumentNumber), request.DocumentNumber);

        if (await _customerRepository.PhoneExistsAsync(request.Phone, cancellationToken))
            throw new ConflictException(nameof(Customer), nameof(Customer.Phone), request.Phone);

        // 2. Create the ApplicationUser (Customer role) and get temp password
        var (userId, tempPassword) = await _identityService.CreateUserAsync(
            email:     request.Email,
            firstName: request.FirstName,
            lastName:  request.LastName,
            role:      "Customer",
            cancellationToken: cancellationToken);

        // 3. Create the Customer entity linked to the new user
        var customer = new Customer
        {
            FirstName         = request.FirstName,
            LastName          = request.LastName,
            DocumentType      = request.DocumentType,
            DocumentNumber    = request.DocumentNumber,
            Phone             = request.Phone,
            Email             = request.Email,
            ApplicationUserId = userId,
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<CustomerDto>(customer);
        return new CreateCustomerResponseDto(dto, tempPassword);
    }
}
