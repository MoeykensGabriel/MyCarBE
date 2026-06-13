using MediatR;
using MyCarBE.Application.Features.Maintenance.DTOs;

namespace MyCarBE.Application.Features.Maintenance.Queries.GetMaintenanceSummary;

/// <summary>
/// Alertas de mantenimiento de todos los vehículos del cliente (o flota) actual.
/// El dueño se deriva del JWT — no recibe ids de afuera.
/// </summary>
public record GetMaintenanceSummaryQuery() : IRequest<IReadOnlyList<MaintenanceAlertDto>>;
