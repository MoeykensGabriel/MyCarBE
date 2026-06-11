using MediatR;
using MyCarBE.Application.Features.VehicleMileage.DTOs;

namespace MyCarBE.Application.Features.VehicleMileage.Queries.GetVehicleMileageReadings;

/// <summary>Historial de lecturas de km del vehículo (las más recientes primero).</summary>
public record GetVehicleMileageReadingsQuery(Guid VehicleId)
    : IRequest<IReadOnlyList<VehicleMileageReadingDto>>;
