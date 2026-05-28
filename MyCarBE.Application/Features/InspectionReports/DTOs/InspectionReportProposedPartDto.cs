namespace MyCarBE.Application.Features.InspectionReports.DTOs;

public record InspectionReportProposedPartDto(
    Guid     Id,
    string   Name,
    int      Quantity,
    string?  ProductCode,
    decimal? EstimatedUnitPrice
);
