using MediatR;
using MyCarBE.Application.Features.InspectionReports.DTOs;

namespace MyCarBE.Application.Features.InspectionReports.Commands.MarkAreaNoFindings;

/// <summary>
/// El admin marca un área como "sin hallazgos" en una orden — útil cuando no hay
/// mecánico asignado a esa área o cuando nadie llegó a reportar. Equivale a un
/// InspectionReport con IsNoFindings=true.
/// </summary>
public record MarkAreaNoFindingsCommand(
    Guid WorkOrderId,
    Guid AreaId
) : IRequest<InspectionReportDto>;
