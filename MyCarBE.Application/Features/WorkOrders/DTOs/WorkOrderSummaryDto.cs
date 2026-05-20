using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

/// <summary>Lightweight DTO used in list responses.</summary>
public record WorkOrderSummaryDto(
    Guid            Id,
    Guid            VehicleId,
    string          VehicleBrand,
    string          VehicleModel,
    string          VehicleLicensePlate,
    Guid?           CustomerIdAtEntry,
    Guid?           FleetIdAtEntry,
    string?         OwnerName,
    int             MileageAtEntry,
    WorkOrderStatus CurrentStatus,
    decimal         TotalAmount,
    string?         CustomerNote,
    string?         TechnicianNote,
    string?         ContactPersonName,
    string?         ContactPersonPhone,
    DateTime        CreatedAt,
    DateTime        UpdatedAt
);
