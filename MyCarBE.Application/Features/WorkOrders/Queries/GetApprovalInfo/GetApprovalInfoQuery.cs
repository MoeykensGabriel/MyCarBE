using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetApprovalInfo;

public record GetApprovalInfoQuery(string Token) : IRequest<ApprovalInfoDto>;
