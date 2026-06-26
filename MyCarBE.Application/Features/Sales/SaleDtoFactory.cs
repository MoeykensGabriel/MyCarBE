using MyCarBE.Application.Features.Sales.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Sales;

/// <summary>
/// Arma el SaleDto: el nombre del comprador se resuelve por join (Customer/Fleet);
/// el del vendedor ya viene snapshoteado en la venta.
/// </summary>
public static class SaleDtoFactory
{
    public static SaleDto Build(Sale sale)
    {
        var buyerName = sale.Customer is not null
            ? $"{sale.Customer.FirstName} {sale.Customer.LastName}".Trim()
            : sale.Fleet?.CompanyName ?? "—";

        var items = sale.Items
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.CreatedAt)
            .Select(i => new SaleItemDto(i.Id, i.ProductCode, i.Name, i.UnitPrice, i.Quantity, i.Subtotal))
            .ToList();

        return new SaleDto(
            sale.Id,
            sale.CustomerId,
            sale.FleetId,
            buyerName,
            sale.SellerUserId,
            sale.SellerName,
            sale.TotalAmount,
            sale.CreatedAt,
            items);
    }
}
