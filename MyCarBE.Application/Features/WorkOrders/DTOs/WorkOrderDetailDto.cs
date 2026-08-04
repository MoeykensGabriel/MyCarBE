using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

/// <summary>
/// Full detail DTO — includes the services list and the complete status timeline.
/// Returned by GetById; used for both admin detail view and customer portal.
/// </summary>
public record WorkOrderDetailDto(
    Guid                               Id,
    /// <summary>Número visible de la orden — es lo que se muestra y se busca. Ver WorkOrder.Number.</summary>
    int                                Number,
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
    /// <summary>Motivo de visita que el cliente reportó al traer el vehículo.</summary>
    string?                            ServiceReason,
    /// <summary>
    /// Vencimiento del presupuesto (14 días desde el envío). Null si aún no se envió.
    /// Vencido: el cliente no puede aprobar y el cleanup cancela la orden.
    /// </summary>
    DateTime?                          QuoteExpiresAt,
    /// <summary>
    /// Condición de venta para los repuestos (CC / OC / Contado). La carga la oficina
    /// antes de aprobar y viaja al depósito con el pedido. OC → PurchaseOrderNumber;
    /// Contado → DepositAmount (seña del cliente).
    /// </summary>
    SaleCondition?                     SaleCondition,
    string?                            PurchaseOrderNumber,
    decimal?                           DepositAmount,
    IReadOnlyList<WorkOrderServiceDto>              Services,
    IReadOnlyList<WorkOrderPartDto>                 Parts,
    IReadOnlyList<WorkOrderPhotoDto>                Photos,
    IReadOnlyList<WorkOrderStatusChangeDto>         Timeline,
    /// <summary>
    /// Vista liviana de los reportes de inspección de la orden (Id + área + mecánico).
    /// Sirve al FE para emparejar fotos de inspección con fotos del servicio del mismo área.
    /// Para detalle completo del reporte (findings, propuestas) usar el endpoint de InspectionReports.
    /// </summary>
    IReadOnlyList<WorkOrderInspectionReportLiteDto> InspectionReports,
    /// <summary>Agendado del vehículo (ocupación de bahía). Null si la orden no está agendada.</summary>
    DateTime?                          ScheduledStart,
    DateTime?                          ScheduledEnd,
    /// <summary>
    /// Duración total estimada de los servicios activos, en minutos:
    /// suma de (estimación del mecánico ?? duración del catálogo) * cantidad.
    /// Sirve para el agendado automático y para mostrar el ETA total.
    /// </summary>
    int                                TotalEstimatedMinutes,

    /// <summary>Para qué entró el vehículo. Se congela en el ingreso.</summary>
    WorkOrderPurpose                   Purpose = WorkOrderPurpose.Repair,

    /// <summary>
    /// Sigue siendo solo inspección (no se promovió). Es lo que hay que mirar para decidir
    /// qué mostrar: Purpose solo dice cómo ENTRÓ.
    /// </summary>
    bool                               IsInspectionOnly = false,

    /// <summary>Cuándo se promovió a orden de trabajo. Null si nunca se promovió.</summary>
    DateTime?                          PromotedToRepairAt = null
);
