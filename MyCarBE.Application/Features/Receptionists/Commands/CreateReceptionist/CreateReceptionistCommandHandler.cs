using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Receptionists.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Receptionists.Commands.CreateReceptionist;

public class CreateReceptionistCommandHandler : IRequestHandler<CreateReceptionistCommand, CreateReceptionistResponseDto>
{
    private readonly IReceptionistRepository _repository;
    private readonly IIdentityService        _identityService;
    private readonly IUnitOfWork             _unitOfWork;
    private readonly IMapper                 _mapper;

    public CreateReceptionistCommandHandler(
        IReceptionistRepository repository,
        IIdentityService        identityService,
        IUnitOfWork             unitOfWork,
        IMapper                 mapper)
    {
        _repository      = repository;
        _identityService = identityService;
        _unitOfWork      = unitOfWork;
        _mapper          = mapper;
    }

    public async Task<CreateReceptionistResponseDto> Handle(CreateReceptionistCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _repository.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException(nameof(Receptionist), nameof(request.Email), email);

        var (userId, tempPassword) = await _identityService.CreateUserAsync(
            email,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            role: "Receptionist",
            cancellationToken);

        var receptionist = new Receptionist
        {
            Id                = Guid.NewGuid(),
            FirstName         = request.FirstName.Trim(),
            LastName          = request.LastName.Trim(),
            Email             = email,
            IsActive          = true,
            ApplicationUserId = userId,
        };

        await _repository.AddAsync(receptionist, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateReceptionistResponseDto(_mapper.Map<ReceptionistDto>(receptionist), tempPassword);
    }
}
