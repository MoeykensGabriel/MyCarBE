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
    IReadOnlyList<WorkOrderServiceDto>              Services,
    IReadOnlyList<WorkOrderPartDto>                 Parts,
    IReadOnlyList<WorkOrderPhotoDto>                Photos,
    IReadOnlyList<WorkOrderStatusChangeDto>         Timeline,
    /// <summary>
    /// Vista liviana de los reportes de inspección de la orden (Id + área + mecánico).
    /// Sirve al FE para emparejar fotos de inspección con fotos del servicio del mismo área.
    /// Para detalle completo del reporte (findings, propuestas) usar el endpoint de InspectionReports.
    /// </summary>
    IReadOnlyList<WorkOrderInspectionReportLiteDto> InspectionReports
);
