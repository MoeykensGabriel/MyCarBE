using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetQuoteApprovalLink;

public record GetQuoteApprovalLinkQuery(Guid WorkOrderId) : IRequest<QuoteApprovalLinkDto>;
