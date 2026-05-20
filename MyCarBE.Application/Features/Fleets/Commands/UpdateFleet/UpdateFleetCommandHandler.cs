using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Validation;
using MyCarBE.Application.Features.Fleets.DTOs;

namespace MyCarBE.Application.Features.Fleets.Commands.UpdateFleet;

public class UpdateFleetCommandHandler : IRequestHandler<UpdateFleetCommand, FleetDto>
{
    private readonly IFleetRepository _fleetRepository;
    private readonly IUnitOfWork      _unitOfWork;
    private readonly IMapper          _mapper;

    public UpdateFleetCommandHandler(IFleetRepository fleetRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _fleetRepository = fleetRepository;
        _unitOfWork      = unitOfWork;
        _mapper          = mapper;
    }

    public async Task<FleetDto> Handle(UpdateFleetCommand request, CancellationToken cancellationToken)
    {
        var fleet = await _fleetRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Fleet), request.Id);

        var normalizedTaxId = ArgentinaIdentifiers.NormalizeCuit(request.TaxId);
        var normalizedPhone = ArgentinaIdentifiers.NormalizePhone(request.Phone);

        if (await _fleetRepository.TaxIdExistsAsync(normalizedTaxId, request.Id, cancellationToken))
            throw new ConflictException(nameof(Domain.Entities.Fleet), nameof(Domain.Entities.Fleet.TaxId), normalizedTaxId);

        fleet.CompanyName = request.CompanyName.Trim();
        fleet.TaxId       = normalizedTaxId;
        fleet.Phone       = normalizedPhone;
        fleet.Email       = string.IsNullOrWhiteSpace(request.Email)   ? null : request.Email.Trim();
        fleet.Address     = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

        _fleetRepository.Update(fleet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FleetDto>(fleet);
    }
}
