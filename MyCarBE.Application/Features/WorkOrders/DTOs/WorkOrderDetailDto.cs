using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

/// <summary>
/// Full detail DTO — includes the services list and the complete status timeline.
/// Returned by GetById; used for both admin detail view and customer portal.
/// </summary>
public record WorkOrderDetailDto(
    Guid                               Id,
    Guid                               VehicleId,
    Guid?                              CustomerIdAtEntry,
    Guid?                              FleetIdAtEntry,
    int                                MileageAtEntry,
    WorkOrderStatus                    CurrentStatus,
    decimal                            TotalAmount,
    string?                            CustomerNote,
    string?                            TechnicianNote,
    DateTime                           CreatedAt,
    DateTime                           UpdatedAt,
    IReadOnlyList<WorkOrderServiceDto> Services,
    IReadOnlyList<WorkOrderStatusChangeDto> Timeline
);
