using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Validation;
using MyCarBE.Application.Features.Customers.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResponseDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IIdentityService    _identityService;
    private readonly IEmailService       _emailService;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly IMapper             _mapper;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IIdentityService    identityService,
        IEmailService       emailService,
        IUnitOfWork         unitOfWork,
        IMapper             mapper,
        ILogger<CreateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _identityService    = identityService;
        _emailService       = emailService;
        _unitOfWork         = unitOfWork;
        _mapper             = mapper;
        _logger             = logger;
    }

    public async Task<CreateCustomerResponseDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // 0. Normalizar a formato canónico antes de chequear unicidad y guardar.
        // Así "12.345.678" y "12345678" se tratan como el mismo DNI.
        var normalizedDocument = request.DocumentType switch
        {
            DocumentType.DNI                          => ArgentinaIdentifiers.NormalizeDni(request.DocumentNumber),
            DocumentType.CUIT or DocumentType.CUIL    => ArgentinaIdentifiers.NormalizeCuit(request.DocumentNumber),
            DocumentType.Passport                     => ArgentinaIdentifiers.NormalizePassport(request.DocumentNumber),
            _                                         => request.DocumentNumber.Trim(),
        };
        var normalizedPhone = ArgentinaIdentifiers.NormalizePhone(request.Phone);

        // 1. Uniqueness checks (sobre los valores normalizados)
        if (await _customerRepository.DocumentNumberExistsAsync(normalizedDocument, cancellationToken))
            throw new ConflictException(nameof(Customer), nameof(Customer.DocumentNumber), normalizedDocument);

        if (await _customerRepository.PhoneExistsAsync(normalizedPhone, cancellationToken))
            throw new ConflictException(nameof(Customer), nameof(Customer.Phone), normalizedPhone);

        // Una flota solo puede tener un contacto encargado
        if (request.FleetId.HasValue && await _customerRepository.FleetContactExistsAsync(request.FleetId.Value, cancellationToken))
            throw new BadRequestException("La flota ya tiene un contacto encargado asignado. Una flota solo puede tener un encargado.");

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
            FirstName         = request.FirstName.Trim(),
            LastName          = request.LastName.Trim(),
            DocumentType      = request.DocumentType,
            DocumentNumber    = normalizedDocument,
            Phone             = normalizedPhone,
            Email             = request.Email.Trim(),
            ApplicationUserId = userId,
            FleetId           = request.FleetId,
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Enviar credenciales por email. Fire-and-forget para no bloquear al admin,
        // pero CON try/catch + log — sin esto, un fallo de SMTP (contraseña mal
        // configurada, etc.) desaparece sin dejar rastro y el cliente nunca sabe
        // que no le llegaron sus credenciales.
        _ = SendWelcomeEmailAsync(request.Email, request.FirstName, tempPassword);

        var dto = _mapper.Map<CustomerDto>(customer);
        return new CreateCustomerResponseDto(dto, tempPassword);
    }

    private async Task SendWelcomeEmailAsync(string email, string firstName, string tempPassword)
    {
        try
        {
            await _emailService.SendAsync(
                to:       email,
                subject:  "Bienvenido a GB Service — tus credenciales de acceso",
                htmlBody: BuildWelcomeEmail(firstName, email, tempPassword),
                cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email de bienvenida a {Email}", email);
        }
    }

    private static string BuildWelcomeEmail(string firstName, string email, string tempPassword) => $"""
        <h2>Hola, {firstName}!</h2>
        <p>Tu cuenta en <strong>GB Service</strong> fue creada. Podés acceder con las siguientes credenciales:</p>
        <ul>
          <li><strong>Email:</strong> {email}</li>
          <li><strong>Contraseña temporal:</strong> {tempPassword}</li>
        </ul>
        <p>Por seguridad, cambiá tu contraseña en el primer ingreso.</p>
        """;
}
