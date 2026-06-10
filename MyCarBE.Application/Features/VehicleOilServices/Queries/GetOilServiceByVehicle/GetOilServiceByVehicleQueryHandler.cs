using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // reusa VehicleOwnershipGuard
using MyCarBE.Application.Features.VehicleOilServices.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleOilServices.Queries.GetOilServiceByVehicle;

public class GetOilServiceByVehicleQueryHandler
    : IRequestHandler<GetOilServiceByVehicleQuery, VehicleOilServiceDto?>
{
    // Umbrales de "próximo" — el aviso salta cuando falta poco por km o por tiempo.
    private const int DueSoonKm   = 1000;
    private const int DueSoonDays = 30;

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

        var nextServiceKm = oil.ChangedAtKm + oil.IntervalKm;
        var nextServiceOn = oil.ChangedOn.AddMonths(oil.IntervalMonths);

        var today         = DateOnly.FromDateTime(DateTime.UtcNow);
        var kmRemaining   = nextServiceKm - vehicle.CurrentMileage;
        var daysRemaining = nextServiceOn.DayNumber - today.DayNumber;

        // El contador que llegue primero manda. Vencido si cualquiera ya se cumplió.
        var status =
            (kmRemaining <= 0 || daysRemaining <= 0)             ? OilServiceStatus.Overdue
            : (kmRemaining <= DueSoonKm || daysRemaining <= DueSoonDays) ? OilServiceStatus.DueSoon
            : OilServiceStatus.Ok;

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
            NextServiceKm:  nextServiceKm,
            NextServiceOn:  nextServiceOn,
            CurrentMileage: vehicle.CurrentMileage,
            KmRemaining:    kmRemaining,
            DaysRemaining:  daysRemaining,
            Status:         status,
            CreatedAt:      oil.CreatedAt
        );
    }
}
