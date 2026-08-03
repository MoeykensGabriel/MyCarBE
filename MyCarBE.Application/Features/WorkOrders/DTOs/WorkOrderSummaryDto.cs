using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

/// <summary>Lightweight DTO used in list responses.</summary>
public record WorkOrderSummaryDto(
    Guid            Id,
    /// <summary>Número visible de la orden — es lo que se muestra y se busca. Ver WorkOrder.Number.</summary>
    int             Number,
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
    DateTime        UpdatedAt,

    /// <summary>
    /// Llegó un hallazgo con la inspección ya cerrada. Es el aviso para la oficina: puede
    /// cambiar un presupuesto en armado, o exigir que se le pida al cliente un adicional.
    /// </summary>
    bool            HasLateFindings = false
);
