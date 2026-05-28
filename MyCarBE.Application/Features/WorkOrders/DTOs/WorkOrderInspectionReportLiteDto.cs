namespace MyCarBE.Application.Features.WorkOrders.DTOs;

/// <summary>
/// Versión liviana de InspectionReport para que el detalle de la orden la incluya.
/// Solo lleva lo que el FE necesita para emparejar fotos antes/después por área.
/// El feature de InspectionReports tiene su propio DTO completo (findings, propuestas, etc.).
/// </summary>
public record WorkOrderInspectionReportLiteDto(
    Guid    Id,
    Guid    AreaId,
    string  AreaName,
    Guid?   MechanicId,
    string? MechanicFullName,
    bool    HasIssue
);
