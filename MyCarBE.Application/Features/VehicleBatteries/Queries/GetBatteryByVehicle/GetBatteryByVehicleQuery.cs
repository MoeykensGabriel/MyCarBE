using MediatR;
using MyCarBE.Application.Features.VehicleBatteries.DTOs;

namespace MyCarBE.Application.Features.VehicleBatteries.Queries.GetBatteryByVehicle;

/// <summary>
/// Devuelve la batería activa del vehículo (con su historial de chequeos), o null si no hay.
/// Pasar includeReplaced=true para incluir baterías reemplazadas.
/// </summary>
public record GetBatteryByVehicleQuery(
    Guid VehicleId,
    bool IncludeReplaced = false
) : IRequest<VehicleBatteryDto?>;
