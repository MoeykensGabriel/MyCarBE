using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Validation;
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
        // Normalizamos a formato canónico para el chequeo de unicidad y para guardar.
        var normalizedTaxId = ArgentinaIdentifiers.NormalizeCuit(request.TaxId);
        var normalizedPhone = ArgentinaIdentifiers.NormalizePhone(request.Phone);

        if (await _fleetRepository.TaxIdExistsAsync(normalizedTaxId, cancellationToken))
            throw new ConflictException(nameof(Fleet), nameof(Fleet.TaxId), normalizedTaxId);

        var fleet = new Fleet
        {
            CompanyName = request.CompanyName.Trim(),
            TaxId       = normalizedTaxId,
            Phone       = normalizedPhone,
            Email       = string.IsNullOrWhiteSpace(request.Email)   ? null : request.Email.Trim(),
            Address     = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
        };

        await _fleetRepository.AddAsync(fleet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return fleet.Id;
    }
}
