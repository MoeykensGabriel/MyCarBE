namespace MyCarBE.Application.Features.Sales.DTOs;

/// <summary>
/// Una venta de repuestos. BuyerName/SellerName ya vienen resueltos (comprador por join,
/// vendedor snapshoteado) para que el front no tenga que cruzar nada.
/// </summary>
public record SaleDto(
    Guid                       Id,
    Guid?                      CustomerId,
    Guid?                      FleetId,
    string                     BuyerName,
    Guid                       SellerUserId,
    string                     SellerName,
    decimal                    TotalAmount,
    DateTime                   CreatedAt,
    IReadOnlyList<SaleItemDto> Items
);
