using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.StockRequests.DTOs;
using MyCarBE.Application.Features.StockRequests.Queries.GetStockRequests;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.StockRequests.Queries.GetStockRequestById;

public class GetStockRequestByIdQueryHandler
    : IRequestHandler<GetStockRequestByIdQuery, StockRequestDto>
{
    private readonly IPartsStockRequestRepository _repository;

    public GetStockRequestByIdQueryHandler(IPartsStockRequestRepository repository)
        => _repository = repository;

    public async Task<StockRequestDto> Handle(
        GetStockRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var stockRequest = await _repository.GetWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PartsStockRequest), request.Id);

        return GetStockRequestsQueryHandler.ToDto(stockRequest);
    }
}
