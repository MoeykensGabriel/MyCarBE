namespace MyCarBE.Application.Features.WorkOrders.DTOs;

public record ApprovalInfoDto(
    Guid    WorkOrderId,
    string  VehicleLicensePlate,
    string  VehicleBrand,
    string  VehicleModel,
    int     VehicleYear,
    string  CustomerName,
    decimal TotalAmount,
    IReadOnlyList<ApprovalServiceItemDto> Services,
    DateTime ExpiresAt,
    bool    IsExpired
);

public record ApprovalServiceItemDto(
    Guid    Id,
    string  Name,
    string? Description,
    decimal UnitPrice,
    int     Quantity,
    decimal Subtotal
);
