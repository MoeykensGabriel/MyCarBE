using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // VehicleOwnershipGuard (compartido)
using MyCarBE.Application.Features.VehicleMileage.DTOs;

namespace MyCarBE.Application.Features.VehicleMileage.Queries.GetVehicleMileageReadings;

public class GetVehicleMileageReadingsQueryHandler
    : IRequestHandler<GetVehicleMileageReadingsQuery, IReadOnlyList<VehicleMileageReadingDto>>
{
    // La trazabilidad útil son las últimas semanas/meses — no hace falta paginar (todavía).
    private const int MaxReadings = 50;

    private readonly IVehicleMileageReadingRepository _readingRepository;
    private readonly IVehicleRepository               _vehicleRepository;
    private readonly ICurrentUserService              _currentUser;

    public GetVehicleMileageReadingsQueryHandler(
        IVehicleMileageReadingRepository readingRepository,
        IVehicleRepository               vehicleRepository,
        ICurrentUserService              currentUser)
    {
        _readingRepository = readingRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
    }

    public async Task<IReadOnlyList<VehicleMileageReadingDto>> Handle(
        GetVehicleMileageReadingsQuery request, CancellationToken cancellationToken)
    {
        await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        var readings = await _readingRepository.GetLatestByVehicleAsync(
            request.VehicleId, MaxReadings, cancellationToken);

        return readings
            .Select(r => new VehicleMileageReadingDto(r.Id, r.Mileage, r.Source, r.CreatedAt))
            .ToList();
    }
}
