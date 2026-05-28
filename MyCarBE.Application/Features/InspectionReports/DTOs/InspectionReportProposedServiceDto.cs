namespace MyCarBE.Application.Features.InspectionReports.DTOs;

public record InspectionReportProposedServiceDto(
    Guid    Id,
    string  Name,
    string? Description,
    decimal EstimatedLaborCost,
    int?    EstimatedDays
);
