using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Receptionists.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Receptionists.Commands.UpdateReceptionist;

public class UpdateReceptionistCommandHandler : IRequestHandler<UpdateReceptionistCommand, ReceptionistDto>
{
    private readonly IReceptionistRepository _repository;
    private readonly IUnitOfWork             _unitOfWork;
    private readonly IMapper                 _mapper;

    public UpdateReceptionistCommandHandler(
        IReceptionistRepository repository,
        IUnitOfWork             unitOfWork,
        IMapper                 mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    public async Task<ReceptionistDto> Handle(UpdateReceptionistCommand request, CancellationToken cancellationToken)
    {
        var receptionist = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Receptionist), request.Id);

        receptionist.FirstName = request.FirstName.Trim();
        receptionist.LastName  = request.LastName.Trim();
        receptionist.IsActive  = request.IsActive;

        _repository.Update(receptionist);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ReceptionistDto>(receptionist);
    }
}
