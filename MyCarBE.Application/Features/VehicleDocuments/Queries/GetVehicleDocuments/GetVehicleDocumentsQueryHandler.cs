using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;

namespace MyCarBE.Application.Features.VehicleDocuments.Queries.GetVehicleDocuments;

public class GetVehicleDocumentsQueryHandler
    : IRequestHandler<GetVehicleDocumentsQuery, IReadOnlyList<VehicleDocumentDto>>
{
    private readonly IVehicleDocumentRepository _docRepository;
    private readonly IVehicleRepository         _vehicleRepository;
    private readonly ICurrentUserService        _currentUser;
    private readonly IMapper                    _mapper;

    public GetVehicleDocumentsQueryHandler(
        IVehicleDocumentRepository docRepository,
        IVehicleRepository         vehicleRepository,
        ICurrentUserService        currentUser,
        IMapper                    mapper)
    {
        _docRepository     = docRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
        _mapper            = mapper;
    }

    public async Task<IReadOnlyList<VehicleDocumentDto>> Handle(GetVehicleDocumentsQuery request, CancellationToken cancellationToken)
    {
        await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        var docs = await _docRepository.GetByVehicleAsync(request.VehicleId, cancellationToken);
        return docs.Select(d => _mapper.Map<VehicleDocumentDto>(d)).ToList();
    }
}
