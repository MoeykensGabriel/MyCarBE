using MediatR;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Vehicles.DTOs;

namespace MyCarBE.Application.Features.Vehicles.Queries.GetVehiclesByOwner;

/// <summary>
/// Búsqueda paginada de vehículos.
/// Admin: puede filtrar por customerId, fleetId y/o search (patente, marca, modelo).
/// Customer: ignora todos los filtros — ve solo sus propios vehículos por JWT.
/// </summary>
public record GetVehiclesByOwnerQuery(
    Guid?  CustomerId = null,
    Guid?  FleetId    = null,
    string? Search    = null,
    int    Page       = 1,
    int    PageSize   = 20
) : IRequest<PagedResult<VehicleDto>>;
