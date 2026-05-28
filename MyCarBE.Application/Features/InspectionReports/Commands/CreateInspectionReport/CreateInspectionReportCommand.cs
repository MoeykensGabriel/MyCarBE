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
    IReadOnlyList<ProposedPartInput>?    ProposedParts    = null
) : IRequest<InspectionReportDto>;
