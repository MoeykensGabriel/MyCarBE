using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Sales.DTOs;

namespace MyCarBE.Application.Features.Sales.Queries.GetSales;

public class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, PagedResult<SaleDto>>
{
    private readonly ISaleRepository _saleRepository;

    public GetSalesQueryHandler(ISaleRepository saleRepository) => _saleRepository = saleRepository;

    public async Task<PagedResult<SaleDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var paged = await _saleRepository.GetPagedAsync(
            request.CustomerId, request.FleetId, request.SellerUserId,
            request.From, request.To, page, pageSize, cancellationToken);

        var items = paged.Items.Select(SaleDtoFactory.Build).ToList();
        return new PagedResult<SaleDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
