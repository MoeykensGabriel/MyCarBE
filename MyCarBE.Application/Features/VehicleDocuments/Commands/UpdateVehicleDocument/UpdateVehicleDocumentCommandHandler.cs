using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleDocuments.Commands.UpdateVehicleDocument;

public class UpdateVehicleDocumentCommandHandler
    : IRequestHandler<UpdateVehicleDocumentCommand, VehicleDocumentDto>
{
    private readonly IVehicleDocumentRepository _docRepository;
    private readonly IVehicleRepository         _vehicleRepository;
    private readonly ICurrentUserService        _currentUser;
    private readonly IUnitOfWork                _unitOfWork;
    private readonly IMapper                    _mapper;

    public UpdateVehicleDocumentCommandHandler(
        IVehicleDocumentRepository docRepository,
        IVehicleRepository         vehicleRepository,
        ICurrentUserService        currentUser,
        IUnitOfWork                unitOfWork,
        IMapper                    mapper)
    {
        _docRepository     = docRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
        _unitOfWork        = unitOfWork;
        _mapper            = mapper;
    }

    public async Task<VehicleDocumentDto> Handle(UpdateVehicleDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _docRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(VehicleDocument), request.Id);

        await VehicleOwnershipGuard.EnsureAccessAsync(
            doc.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        doc.DocumentType  = request.DocumentType;
        doc.ExpiresOn     = request.ExpiresOn;
        doc.Notes         = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        doc.IssuingEntity = string.IsNullOrWhiteSpace(request.IssuingEntity) ? null : request.IssuingEntity.Trim();

        _docRepository.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<VehicleDocumentDto>(doc);
    }
}
