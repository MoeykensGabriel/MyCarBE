using MediatR;
using MyCarBE.Application.Features.InspectionReports.DTOs;

namespace MyCarBE.Application.Features.InspectionReports.Queries.GetInspectionReportsByWorkOrder;

public record GetInspectionReportsByWorkOrderQuery(Guid WorkOrderId) : IRequest<IReadOnlyList<InspectionReportDto>>;
