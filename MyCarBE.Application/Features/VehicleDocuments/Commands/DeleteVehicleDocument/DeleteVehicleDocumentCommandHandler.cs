using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleDocuments.Commands.DeleteVehicleDocument;

public class DeleteVehicleDocumentCommandHandler : IRequestHandler<DeleteVehicleDocumentCommand>
{
    private readonly IVehicleDocumentRepository _docRepository;
    private readonly IVehicleRepository         _vehicleRepository;
    private readonly ICurrentUserService        _currentUser;
    private readonly IUnitOfWork                _unitOfWork;

    public DeleteVehicleDocumentCommandHandler(
        IVehicleDocumentRepository docRepository,
        IVehicleRepository         vehicleRepository,
        ICurrentUserService        currentUser,
        IUnitOfWork                unitOfWork)
    {
        _docRepository     = docRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
        _unitOfWork        = unitOfWork;
    }

    public async Task Handle(DeleteVehicleDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _docRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(VehicleDocument), request.Id);

        await VehicleOwnershipGuard.EnsureAccessAsync(
            doc.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        _docRepository.Delete(doc); // soft delete (interceptor en SaveChangesAsync)
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
