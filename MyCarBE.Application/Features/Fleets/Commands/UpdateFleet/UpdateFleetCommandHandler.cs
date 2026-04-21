using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
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

        if (await _fleetRepository.TaxIdExistsAsync(request.TaxId, request.Id, cancellationToken))
            throw new ConflictException(nameof(Domain.Entities.Fleet), nameof(Domain.Entities.Fleet.TaxId), request.TaxId);

        fleet.CompanyName = request.CompanyName;
        fleet.TaxId       = request.TaxId;
        fleet.Phone       = request.Phone;
        fleet.Email       = request.Email;
        fleet.Address     = request.Address;

        _fleetRepository.Update(fleet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FleetDto>(fleet);
    }
}
