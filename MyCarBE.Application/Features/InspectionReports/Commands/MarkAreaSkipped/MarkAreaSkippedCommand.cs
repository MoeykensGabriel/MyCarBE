using MediatR;
using MyCarBE.Application.Features.InspectionReports.DTOs;

namespace MyCarBE.Application.Features.InspectionReports.Commands.MarkAreaSkipped;

/// <summary>
/// La oficina omite la inspección de un área — el mecánico está ocupado, el cliente
/// apurado, etc. A diferencia de "sin hallazgos", deja constancia de que NADIE revisó
/// el área: crea un InspectionReport con IsSkipped=true, y el área queda marcada para
/// revisar en la próxima visita del vehículo. El motivo es opcional (la oficina posterga
/// de un solo click; puede dejar constancia si quiere).
/// </summary>
public record MarkAreaSkippedCommand(
    Guid    WorkOrderId,
    Guid    AreaId,
    string? Reason
) : IRequest<InspectionReportDto>;
