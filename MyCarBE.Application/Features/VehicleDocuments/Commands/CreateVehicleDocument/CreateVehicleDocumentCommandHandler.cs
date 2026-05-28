using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleDocuments.Commands.CreateVehicleDocument;

public class CreateVehicleDocumentCommandHandler
    : IRequestHandler<CreateVehicleDocumentCommand, VehicleDocumentDto>
{
    private readonly IVehicleDocumentRepository _docRepository;
    private readonly IVehicleRepository         _vehicleRepository;
    private readonly ICurrentUserService        _currentUser;
    private readonly IUnitOfWork                _unitOfWork;
    private readonly IMapper                    _mapper;

    public CreateVehicleDocumentCommandHandler(
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

    public async Task<VehicleDocumentDto> Handle(CreateVehicleDocumentCommand request, CancellationToken cancellationToken)
    {
        await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        var doc = new VehicleDocument
        {
            Id            = Guid.NewGuid(),
            VehicleId     = request.VehicleId,
            DocumentType  = request.DocumentType,
            ExpiresOn     = request.ExpiresOn,
            Notes         = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IssuingEntity = string.IsNullOrWhiteSpace(request.IssuingEntity) ? null : request.IssuingEntity.Trim(),
        };

        await _docRepository.AddAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<VehicleDocumentDto>(doc);
    }
}
