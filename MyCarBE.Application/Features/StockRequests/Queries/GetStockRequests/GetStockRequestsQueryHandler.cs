using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.StockRequests.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.StockRequests.Queries.GetStockRequests;

public class GetStockRequestsQueryHandler
    : IRequestHandler<GetStockRequestsQuery, IReadOnlyList<StockRequestDto>>
{
    private readonly IPartsStockRequestRepository _repository;

    public GetStockRequestsQueryHandler(IPartsStockRequestRepository repository)
        => _repository = repository;

    public async Task<IReadOnlyList<StockRequestDto>> Handle(
        GetStockRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var requests = await _repository.GetAllFilteredAsync(
            request.Status, request.LicensePlate, cancellationToken);

        return requests.Select(ToDto).ToList();
    }

    internal static StockRequestDto ToDto(PartsStockRequest r) => new(
        Id:                r.Id,
        WorkOrderId:       r.WorkOrderId,
        LicensePlate:      r.LicensePlateSnapshot,
        ExternalReference: r.ExternalReference,
        Status:            r.Status,
        CreatedAt:         r.CreatedAt,
        UpdatedAt:         r.UpdatedAt,
        VehicleBrand:      r.WorkOrder?.Vehicle?.Brand,
        VehicleModel:      r.WorkOrder?.Vehicle?.Model,
        Items: r.Items
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.Name)
            .Select(i => new StockRequestItemDto(
                Id:              i.Id,
                WorkOrderPartId: i.WorkOrderPartId,
                ProductCode:     i.ProductCode,
                Name:             i.Name,
                Quantity:        i.Quantity,
                Status:          i.Status,
                Notes:           i.Notes,
                UpdatedAt:       i.UpdatedAt))
            .ToList());
}
