using MediatR;
using MyCarBE.Application.Features.Dashboard.DTOs;

namespace MyCarBE.Application.Features.Dashboard.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
