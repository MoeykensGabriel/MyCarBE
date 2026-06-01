using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

public record WorkOrderPartDto(
    Guid                    Id,
    string?                 ProductCode,
    string                  Name,
    decimal                 UnitPrice,
    decimal?                CustomerUnitPrice,
    int                     Quantity,
    decimal                 Subtotal,
    decimal                 CustomerSubtotal,
    WorkOrderPartTier       Tier,
    Guid?                   AlternativeGroupId,
    QuoteItemApprovalStatus ApprovalStatus,
    DateTime?               FrozenAt,
    DateTime                CreatedAt,
    DateTime                UpdatedAt
);
