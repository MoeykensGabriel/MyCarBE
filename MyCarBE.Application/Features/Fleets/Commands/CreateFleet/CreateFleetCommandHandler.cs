using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Fleets.Commands.CreateFleet;

public class CreateFleetCommandHandler : IRequestHandler<CreateFleetCommand, Guid>
{
    private readonly IFleetRepository _fleetRepository;
    private readonly IUnitOfWork      _unitOfWork;

    public CreateFleetCommandHandler(IFleetRepository fleetRepository, IUnitOfWork unitOfWork)
    {
        _fleetRepository = fleetRepository;
        _unitOfWork      = unitOfWork;
    }

    public async Task<Guid> Handle(CreateFleetCommand request, CancellationToken cancellationToken)
    {
        if (await _fleetRepository.TaxIdExistsAsync(request.TaxId, cancellationToken))
            throw new ConflictException(nameof(Fleet), nameof(Fleet.TaxId), request.TaxId);

        var fleet = new Fleet
        {
            CompanyName = request.CompanyName,
            TaxId       = request.TaxId,
            Phone       = request.Phone,
            Email       = request.Email,
            Address     = request.Address,
        };

        await _fleetRepository.AddAsync(fleet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return fleet.Id;
    }
}
