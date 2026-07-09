using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMyAvailableServices;

public class GetMyAvailableServicesQueryHandler
    : IRequestHandler<GetMyAvailableServicesQuery, IReadOnlyList<AvailableServiceDto>>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ICurrentUserService  _currentUser;

    public GetMyAvailableServicesQueryHandler(
        IWorkOrderRepository workOrderRepository,
        ICurrentUserService  currentUser)
    {
        _workOrderRepository = workOrderRepository;
        _currentUser         = currentUser;
    }

    public async Task<IReadOnlyList<AvailableServiceDto>> Handle(
        GetMyAvailableServicesQuery request,
        CancellationToken cancellationToken)
    {
        // Solo mecánicos — gateway sanity check, además del [Authorize] del controller.
        if (_currentUser.MechanicId is null)
            throw new ForbiddenException("Solo los mecánicos pueden consultar el pool de trabajos disponibles.");

        var services = await _workOrderRepository.GetAvailableServicesAsync(cancellationToken);

        // Sin propietario/cliente/flota: el mecánico no debe saber para quién es el trabajo.
        return services.Select(s =>
            new AvailableServiceDto(
                WorkOrderServiceId:       s.Id,
                WorkOrderId:              s.WorkOrderId,
                ServiceName:              s.NameSnapshot,
                ServiceDescription:       string.IsNullOrWhiteSpace(s.DescriptionSnapshot) ? null : s.DescriptionSnapshot,
                Quantity:                 s.Quantity,
                PriceSnapshot:            s.PriceSnapshot,
                EstimatedDurationMinutes: s.EstimatedDurationMinutesSnapshot,
                CreatedAt:                s.CreatedAt,
                VehicleId:                s.WorkOrder.Vehicle.Id,
                VehicleBrand:             s.WorkOrder.Vehicle.Brand,
                VehicleModel:             s.WorkOrder.Vehicle.Model,
                VehicleLicensePlate:      s.WorkOrder.Vehicle.LicensePlate
            )).ToList();
    }
}
