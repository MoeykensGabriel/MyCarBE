using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // reusa VehicleOwnershipGuard
using MyCarBE.Application.Features.VehicleOilServices.DTOs;

namespace MyCarBE.Application.Features.VehicleOilServices.Queries.GetOilServiceByVehicle;

public class GetOilServiceByVehicleQueryHandler
    : IRequestHandler<GetOilServiceByVehicleQuery, VehicleOilServiceDto?>
{
    private readonly IVehicleOilServiceRepository _oilRepository;
    private readonly IVehicleRepository           _vehicleRepository;
    private readonly ICurrentUserService          _currentUser;

    public GetOilServiceByVehicleQueryHandler(
        IVehicleOilServiceRepository oilRepository,
        IVehicleRepository           vehicleRepository,
        ICurrentUserService          currentUser)
    {
        _oilRepository     = oilRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
    }

    public async Task<VehicleOilServiceDto?> Handle(GetOilServiceByVehicleQuery request, CancellationToken cancellationToken)
    {
        // Valida ownership (Admin / Customer dueño / Fleet Contact) y nos da el vehículo (km actual).
        var vehicle = await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        var oil = await _oilRepository.GetLatestByVehicleAsync(request.VehicleId, cancellationToken);
        if (oil is null) return null;

        var eval = OilServiceStatusCalculator.Evaluate(oil, vehicle.CurrentMileage);

        return new VehicleOilServiceDto(
            Id:             oil.Id,
            VehicleId:      oil.VehicleId,
            ChangedOn:      oil.ChangedOn,
            ChangedAtKm:    oil.ChangedAtKm,
            IntervalKm:     oil.IntervalKm,
            IntervalMonths: oil.IntervalMonths,
            OilType:        oil.OilType,
            OilBrand:       oil.OilBrand,
            FilterChanged:  oil.FilterChanged,
            Notes:          oil.Notes,
            NextServiceKm:  eval.NextServiceKm,
            NextServiceOn:  eval.NextServiceOn,
            CurrentMileage: vehicle.CurrentMileage,
            KmRemaining:    eval.KmRemaining,
            DaysRemaining:  eval.DaysRemaining,
            Status:         eval.Status,
            CreatedAt:      oil.CreatedAt
        );
    }
}
