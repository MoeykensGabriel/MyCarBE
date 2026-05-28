using MediatR;
using MyCarBE.Application.Features.StockRequests.DTOs;

namespace MyCarBE.Application.Features.StockRequests.Queries.GetStockRequestById;

public record GetStockRequestByIdQuery(Guid Id) : IRequest<StockRequestDto>;
