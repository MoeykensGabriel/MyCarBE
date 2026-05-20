using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Validation;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Mechanics.Commands.UpdateMechanic;

public class UpdateMechanicCommandHandler : IRequestHandler<UpdateMechanicCommand, MechanicDto>
{
    private readonly IMechanicRepository _repository;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly IMapper             _mapper;

    public UpdateMechanicCommandHandler(
        IMechanicRepository repository,
        IUnitOfWork         unitOfWork,
        IMapper             mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    public async Task<MechanicDto> Handle(UpdateMechanicCommand request, CancellationToken cancellationToken)
    {
        var mechanic = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), request.Id);

        mechanic.FirstName = request.FirstName.Trim();
        mechanic.LastName  = request.LastName.Trim();
        mechanic.Phone     = string.IsNullOrWhiteSpace(request.Phone) ? null : ArgentinaIdentifiers.NormalizePhone(request.Phone);
        mechanic.Specialty = string.IsNullOrWhiteSpace(request.Specialty) ? null : request.Specialty.Trim();
        mechanic.IsActive  = request.IsActive;

        _repository.Update(mechanic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MechanicDto>(mechanic);
    }
}
