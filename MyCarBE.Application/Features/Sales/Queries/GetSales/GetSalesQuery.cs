using MediatR;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Sales.DTOs;

namespace MyCarBE.Application.Features.Sales.Queries.GetSales;

public record GetSalesQuery(
    Guid?     CustomerId   = null,
    Guid?     FleetId      = null,
    Guid?     SellerUserId = null,
    DateTime? From         = null,
    DateTime? To           = null,
    int       Page         = 1,
    int       PageSize     = 20
) : IRequest<PagedResult<SaleDto>>;
