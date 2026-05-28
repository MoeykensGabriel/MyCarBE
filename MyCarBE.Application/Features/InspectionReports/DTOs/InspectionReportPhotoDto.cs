namespace MyCarBE.Application.Features.InspectionReports.DTOs;

public record InspectionReportPhotoDto(
    Guid     Id,
    string   Url,
    string?  Caption,
    DateTime TakenAt
);
