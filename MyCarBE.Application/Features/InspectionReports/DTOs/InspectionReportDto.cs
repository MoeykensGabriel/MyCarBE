namespace MyCarBE.Application.Features.InspectionReports.DTOs;

public record InspectionReportDto(
    Guid     Id,
    Guid     WorkOrderId,
    Guid     AreaId,
    string   AreaName,
    Guid?    MechanicId,
    string?  MechanicFullName,
    string?  Findings,
    bool     HasIssue,
    bool     IsNoFindings,
    bool     IsSkipped,
    string?  SkipReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<InspectionReportPhotoDto>           Photos,
    IReadOnlyList<InspectionReportProposedServiceDto> ProposedServices,
    IReadOnlyList<InspectionReportProposedPartDto>    ProposedParts
);
