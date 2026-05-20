using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

/// <summary>
/// Full detail DTO — includes the services list and the complete status timeline.
/// Returned by GetById; used for both admin detail view and customer portal.
/// </summary>
public record WorkOrderDetailDto(
    Guid                               Id,
    Guid                               VehicleId,
    string                             VehicleBrand,
    string                             VehicleModel,
    string                             VehicleLicensePlate,
    Guid?                              CustomerIdAtEntry,
    Guid?                              FleetIdAtEntry,
    string?                            OwnerName,
    int                                MileageAtEntry,
    WorkOrderStatus                    CurrentStatus,
    decimal                            TotalAmount,
    string?                            CustomerNote,
    string?                            TechnicianNote,
    string?                            ContactPersonName,
    string?                            ContactPersonPhone,
    DateTime                           CreatedAt,
    DateTime                           UpdatedAt,
    IReadOnlyList<WorkOrderServiceDto>      Services,
    IReadOnlyList<WorkOrderPhotoDto>        Photos,
    IReadOnlyList<WorkOrderStatusChangeDto> Timeline
);
