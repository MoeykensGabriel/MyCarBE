using MediatR;
using MyCarBE.Application.Features.InspectionReports.DTOs;

namespace MyCarBE.Application.Features.InspectionReports.Commands.UpdateInspectionReport;

/// <summary>
/// Edición de un reporte existente. Solo lo puede modificar el mecánico que lo creó.
/// Las listas ProposedServices/ProposedParts son REEMPLAZO TOTAL: lo que venga en la
/// request reemplaza a lo que había. Pasar listas vacías borra las propuestas.
/// </summary>
public record UpdateInspectionReportCommand(
    Guid    Id,
    string? Findings,
    bool    HasIssue,
    IReadOnlyList<ProposedServiceInput>? ProposedServices = null,
    IReadOnlyList<ProposedPartInput>?    ProposedParts    = null
) : IRequest<InspectionReportDto>;
