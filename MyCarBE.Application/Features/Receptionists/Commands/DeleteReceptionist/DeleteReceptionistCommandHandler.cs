using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Receptionists.Commands.DeleteReceptionist;

/// <summary>
/// Soft-delete del recepcionista + IsActive=false. A diferencia de Mechanic, no
/// hay servicios asignados que validar — el usuario solo cargó órdenes y esas
/// órdenes quedan ligadas a su userId histórico vía WorkOrder.CreatedByUserId.
/// </summary>
public class DeleteReceptionistCommandHandler : IRequestHandler<DeleteReceptionistCommand>
{
    private readonly IReceptionistRepository _repository;
    private readonly IUnitOfWork             _unitOfWork;

    public DeleteReceptionistCommandHandler(
        IReceptionistRepository repository,
        IUnitOfWork             unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteReceptionistCommand request, CancellationToken cancellationToken)
    {
        var receptionist = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Receptionist), request.Id);

        receptionist.IsActive = false;
        _repository.Delete(receptionist); // soft delete a nivel BaseEntity
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
