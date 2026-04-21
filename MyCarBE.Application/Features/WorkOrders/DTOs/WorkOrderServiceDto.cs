namespace MyCarBE.Application.Features.WorkOrders.DTOs;

public record WorkOrderServiceDto(
    Guid    Id,
    Guid    CatalogServiceId,
    string  NameSnapshot,
    string  DescriptionSnapshot,
    decimal PriceSnapshot,
    int     Quantity,
    decimal Subtotal        // PriceSnapshot * Quantity — computed for frontend convenience
);
