using MediatR;
using MyCarBE.Application.Features.InspectionReports.DTOs;

namespace MyCarBE.Application.Features.InspectionReports.Commands.CreateInspectionReport;

/// <summary>
/// Reporte de un mecánico sobre un área de una orden en fase de inspección.
/// Si HasIssue=true → Findings obligatorio.
/// Si HasIssue=false → "revisé y no hay nada que cotizar en esta área".
///
/// ProposedServices y ProposedParts: lo que el mecánico sugiere para el presupuesto.
/// Solo se aceptan si HasIssue=true. Pueden venir vacíos.
/// </summary>
public record CreateInspectionReportCommand(
    Guid    WorkOrderId,
    Guid    AreaId,
    string? Findings,
    bool    HasIssue,
    IReadOnlyList<ProposedServiceInput>? ProposedServices = null,
    IReadOnlyList<ProposedPartInput>?    ProposedParts    = null,
    // Solo aplica cuando el área del reporte tiene IsTireArea=true (área de cubiertas).
    // Una entrada por posición revisada. Si el área no es de cubiertas, debe venir vacío.
    IReadOnlyList<TireInspectionInput>?  Tires            = null,
    // Solo aplica cuando el área del reporte tiene IsBatteryArea=true (área de batería).
    // El vehículo tiene una sola batería. Si el área no es de batería, debe venir null.
    BatteryInspectionInput?              Battery          = null,
    // Solo aplica cuando el área del reporte tiene IsOilArea=true (área de aceite).
    // Registra un cambio de aceite/filtros. Si el área no es de aceite, debe venir null.
    OilInspectionInput?                  Oil              = null
) : IRequest<InspectionReportDto>;
