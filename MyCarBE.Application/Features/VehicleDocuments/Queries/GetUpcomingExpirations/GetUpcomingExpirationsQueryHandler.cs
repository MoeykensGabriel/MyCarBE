using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;

namespace MyCarBE.Application.Features.VehicleDocuments.Queries.GetUpcomingExpirations;

public class GetUpcomingExpirationsQueryHandler
    : IRequestHandler<GetUpcomingExpirationsQuery, IReadOnlyList<UpcomingExpirationDto>>
{
    private readonly IVehicleDocumentRepository _docRepository;
    private readonly ICurrentUserService        _currentUser;

    public GetUpcomingExpirationsQueryHandler(
        IVehicleDocumentRepository docRepository,
        ICurrentUserService        currentUser)
    {
        _docRepository = docRepository;
        _currentUser   = currentUser;
    }

    public async Task<IReadOnlyList<UpcomingExpirationDto>> Handle(GetUpcomingExpirationsQuery request, CancellationToken cancellationToken)
    {
        var horizon = request.HorizonDays <= 0 ? 60 : Math.Min(request.HorizonDays, 365);

        // Customer: solo lo suyo. Fleet contact: lo de su flota.
        if (_currentUser.FleetId.HasValue)
            return await _docRepository.GetUpcomingForFleetAsync(
                _currentUser.FleetId.Value, horizon, cancellationToken);

        if (_currentUser.CustomerId.HasValue)
            return await _docRepository.GetUpcomingForCustomerAsync(
                _currentUser.CustomerId.Value, horizon, cancellationToken);

        // Admin / Mechanic / Receptionist no tienen un "me" — devolvemos vacío sin error
        // para no romper el badge si lo abre alguien que no corresponde.
        if (_currentUser.IsAdmin || _currentUser.IsMechanic || _currentUser.IsReceptionist)
            return Array.Empty<UpcomingExpirationDto>();

        throw new ForbiddenException("Solo Customer / Fleet Contact tienen vencimientos asociados.");
    }
}
