using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Validation;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Mechanics.Commands.CreateMechanic;

public class CreateMechanicCommandHandler : IRequestHandler<CreateMechanicCommand, CreateMechanicResponseDto>
{
    private readonly IMechanicRepository _repository;
    private readonly IIdentityService    _identityService;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly IMapper             _mapper;

    public CreateMechanicCommandHandler(
        IMechanicRepository repository,
        IIdentityService    identityService,
        IUnitOfWork         unitOfWork,
        IMapper             mapper)
    {
        _repository      = repository;
        _identityService = identityService;
        _unitOfWork      = unitOfWork;
        _mapper          = mapper;
    }

    public async Task<CreateMechanicResponseDto> Handle(CreateMechanicCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _repository.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException(nameof(Mechanic), nameof(request.Email), email);

        // Crea el ApplicationUser con rol Mechanic + contraseña temporal
        var (userId, tempPassword) = await _identityService.CreateUserAsync(
            email,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            role: "Mechanic",
            cancellationToken);

        var mechanic = new Mechanic
        {
            Id                = Guid.NewGuid(),
            FirstName         = request.FirstName.Trim(),
            LastName          = request.LastName.Trim(),
            Email             = email,
            Phone             = string.IsNullOrWhiteSpace(request.Phone) ? null : ArgentinaIdentifiers.NormalizePhone(request.Phone),
            Specialty         = string.IsNullOrWhiteSpace(request.Specialty) ? null : request.Specialty.Trim(),
            IsActive          = true,
            ApplicationUserId = userId,
        };

        await _repository.AddAsync(mechanic, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateMechanicResponseDto(_mapper.Map<MechanicDto>(mechanic), tempPassword);
    }
}
