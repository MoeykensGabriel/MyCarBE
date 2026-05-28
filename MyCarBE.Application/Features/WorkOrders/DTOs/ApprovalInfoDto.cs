using MyCarBE.Domain.Enums;

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
    IReadOnlyList<ApprovalPartItemDto>    Parts,
    DateTime ExpiresAt,
    bool    IsExpired
);

public record ApprovalServiceItemDto(
    Guid    Id,
    string  Name,
    string? Description,
    decimal UnitPrice,
    int     Quantity,
    decimal Subtotal,
    /// <summary>
    /// Si tiene valor, este item pertenece a un grupo de alternativas mutuamente excluyentes.
    /// El cliente debe elegir exactamente uno por grupo.
    /// </summary>
    Guid?   AlternativeGroupId
);

public record ApprovalPartItemDto(
    Guid    Id,
    string  Name,
    string? ProductCode,
    decimal UnitPrice,
    int     Quantity,
    decimal Subtotal,
    WorkOrderPartTier Tier,
    Guid?   AlternativeGroupId
);
