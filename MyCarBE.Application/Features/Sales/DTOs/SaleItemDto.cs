namespace MyCarBE.Application.Features.Sales.DTOs;

public record SaleItemDto(
    Guid    Id,
    string? ProductCode,
    string  Name,
    decimal UnitPrice,
    int     Quantity,
    decimal Subtotal
);
