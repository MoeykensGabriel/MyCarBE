using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Mechanics.Commands.DeleteMechanic;

/// <summary>
/// Soft-delete del mecánico + IsActive=false. No se permite si tiene servicios
/// activos (Pending/Accepted) — el admin debe reasignar primero.
/// </summary>
public class DeleteMechanicCommandHandler : IRequestHandler<DeleteMechanicCommand>
{
    private readonly IMechanicRepository  _repository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;

    public DeleteMechanicCommandHandler(
        IMechanicRepository  repository,
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork)
    {
        _repository          = repository;
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
    }

    public async Task Handle(DeleteMechanicCommand request, CancellationToken cancellationToken)
    {
        var mechanic = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), request.Id);

        // Bloquea si hay servicios pendientes o aceptados
        var activeServices = await _workOrderRepository.GetServicesByMechanicAsync(
            mechanic.Id, status: null, cancellationToken);

        if (activeServices.Any())
            throw new BadRequestException(
                $"El mecánico tiene {activeServices.Count} servicio(s) activo(s). " +
                "Reasignalos antes de eliminar.");

        mechanic.IsActive = false;
        _repository.Delete(mechanic); // soft delete a nivel BaseEntity
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
