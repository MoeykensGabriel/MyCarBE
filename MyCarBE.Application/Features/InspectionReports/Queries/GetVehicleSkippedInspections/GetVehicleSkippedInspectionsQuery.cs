using MediatR;
using MyCarBE.Application.Features.InspectionReports.DTOs;

namespace MyCarBE.Application.Features.InspectionReports.Queries.GetVehicleSkippedInspections;

/// <summary>
/// Áreas omitidas en la última visita (orden no cancelada más reciente) de un vehículo.
/// Vacío si la última visita cubrió todas las áreas.
/// </summary>
public record GetVehicleSkippedInspectionsQuery(Guid VehicleId)
    : IRequest<IReadOnlyList<SkippedInspectionAreaDto>>;
