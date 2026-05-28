using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

public record WorkOrderPartDto(
    Guid                    Id,
    /// <summary>
    /// Código de proveedor en GestionPGB. Null = repuesto custom (no se envía al depósito).
    /// </summary>
    string?                 ProductCode,
    string                  Name,
    decimal                 UnitPrice,
    int                     Quantity,
    decimal                 Subtotal,             // UnitPrice * Quantity — convenience para FE
    WorkOrderPartTier       Tier,
    Guid?                   AlternativeGroupId,
    QuoteItemApprovalStatus ApprovalStatus,
    DateTime?               FrozenAt,
    DateTime                CreatedAt,
    DateTime                UpdatedAt
);
