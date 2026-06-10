using MediatR;
using MyCarBE.Application.Features.VehicleOilServices.DTOs;

namespace MyCarBE.Application.Features.VehicleOilServices.Queries.GetOilServiceByVehicle;

/// <summary>
/// Devuelve el estado del aceite del vehículo (último cambio + estimación del próximo
/// service), o null si todavía no se registró ningún cambio.
/// </summary>
public record GetOilServiceByVehicleQuery(Guid VehicleId) : IRequest<VehicleOilServiceDto?>;
