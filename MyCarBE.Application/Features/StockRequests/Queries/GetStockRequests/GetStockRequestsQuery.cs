using MediatR;
using MyCarBE.Application.Features.StockRequests.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.StockRequests.Queries.GetStockRequests;

public record GetStockRequestsQuery(
    StockRequestStatus? Status,
    string?             LicensePlate) : IRequest<IReadOnlyList<StockRequestDto>>;
