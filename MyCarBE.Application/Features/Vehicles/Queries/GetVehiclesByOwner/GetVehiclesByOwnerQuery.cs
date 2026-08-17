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
    int    PageSize   = 20,
    // Orden del listado: "alphabetical" (marca/modelo), "plate" (patente), o
    // "recent" (más nuevo primero, default histórico). Null = "recent".
    string? Sort      = null,
    // Solo los vehículos que deben actualizar el kilometraje, según el umbral del taller.
    // Lo usa el aviso de "X vehículos necesitan que actualices su kilometraje" para poder
    // mostrar CUÁLES son en vez de dejar al cliente buscándolos en la lista completa.
    bool MileageDueOnly = false
) : IRequest<PagedResult<VehicleDto>>;
