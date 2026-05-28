using MediatR;
using MyCarBE.Application.Features.InspectionReports.DTOs;

namespace MyCarBE.Application.Features.InspectionReports.Queries.GetInspectionReportById;

public record GetInspectionReportByIdQuery(Guid Id) : IRequest<InspectionReportDto>;
