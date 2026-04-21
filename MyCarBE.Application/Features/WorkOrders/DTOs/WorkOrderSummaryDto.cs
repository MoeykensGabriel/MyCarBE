using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

/// <summary>Lightweight DTO used in list responses.</summary>
public record WorkOrderSummaryDto(
    Guid            Id,
    Guid            VehicleId,
    Guid?           CustomerIdAtEntry,
    Guid?           FleetIdAtEntry,
    int             MileageAtEntry,
    WorkOrderStatus CurrentStatus,
    decimal         TotalAmount,
    string?         CustomerNote,
    string?         TechnicianNote,
    DateTime        CreatedAt,
    DateTime        UpdatedAt
);
