namespace MyCarBE.Application.Features.WorkOrders.DTOs;

/// <summary>
/// Versión liviana de InspectionReport para que el detalle de la orden la incluya.
/// Lleva lo que el FE necesita para emparejar fotos antes/después por área y para
/// que el cliente vea las novedades de la inspección a medida que se reportan.
/// NO incluye las propuestas del mecánico (servicios/repuestos con costos estimados):
/// eso es material interno del admin para cotizar — el feature de InspectionReports
/// tiene su propio DTO completo.
/// </summary>
public record WorkOrderInspectionReportLiteDto(
    Guid     Id,
    Guid     AreaId,
    string   AreaName,
    Guid?    MechanicId,
    string?  MechanicFullName,
    bool     HasIssue,
    string?  Findings,
    DateTime CreatedAt
);
