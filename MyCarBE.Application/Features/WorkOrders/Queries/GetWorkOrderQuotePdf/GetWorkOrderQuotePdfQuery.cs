using MediatR;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderQuotePdf;

public record GetWorkOrderQuotePdfQuery(Guid WorkOrderId) : IRequest<byte[]>;
