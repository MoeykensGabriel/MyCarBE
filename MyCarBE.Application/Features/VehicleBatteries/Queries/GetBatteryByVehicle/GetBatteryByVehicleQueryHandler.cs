using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // reusa VehicleOwnershipGuard
using MyCarBE.Application.Features.VehicleBatteries.DTOs;

namespace MyCarBE.Application.Features.VehicleBatteries.Queries.GetBatteryByVehicle;

public class GetBatteryByVehicleQueryHandler
    : IRequestHandler<GetBatteryByVehicleQuery, VehicleBatteryDto?>
{
    private readonly IVehicleBatteryRepository _batteryRepository;
    private readonly IVehicleRepository        _vehicleRepository;
    private readonly ICurrentUserService       _currentUser;
    private readonly IMapper                   _mapper;

    public GetBatteryByVehicleQueryHandler(
        IVehicleBatteryRepository batteryRepository,
        IVehicleRepository        vehicleRepository,
        ICurrentUserService       currentUser,
        IMapper                   mapper)
    {
        _batteryRepository = batteryRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
        _mapper            = mapper;
    }

    public async Task<VehicleBatteryDto?> Handle(GetBatteryByVehicleQuery request, CancellationToken cancellationToken)
    {
        // Valida ownership (Admin / Customer dueño / Fleet Contact) o tira NotFound.
        await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        var batteries = await _batteryRepository.GetByVehicleAsync(
            request.VehicleId, request.IncludeReplaced, cancellationToken);

        // El vehículo tiene una sola batería activa: devolvemos esa (o la primera del historial).
        var battery = batteries.FirstOrDefault(b => b.IsActive) ?? batteries.FirstOrDefault();
        return battery is null ? null : VehicleBatteryDtoFactory.Build(battery, _mapper);
    }
}
