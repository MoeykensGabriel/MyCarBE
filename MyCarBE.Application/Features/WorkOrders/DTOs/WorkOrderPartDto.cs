using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

public record WorkOrderPartDto(
    Guid                    Id,
    string?                 ProductCode,
    string                  Name,
    decimal                 UnitPrice,
    int                     Quantity,
    decimal                 Subtotal,
    QuoteItemApprovalStatus ApprovalStatus,
    DateTime?               FrozenAt,
    DateTime                CreatedAt,
    DateTime                UpdatedAt
);
