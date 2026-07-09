namespace MyCarBE.Application.Features.InspectionReports.DTOs;

/// <summary>
/// Área cuya inspección se omitió en la última visita de un vehículo.
/// Alimenta el aviso "quedó sin inspeccionar" en la ficha del vehículo
/// y en la creación de una nueva orden.
/// </summary>
public record SkippedInspectionAreaDto(
    Guid     WorkOrderId,
    DateTime WorkOrderCreatedAt,
    Guid     AreaId,
    string   AreaName,
    string   SkipReason,
    DateTime SkippedAt
);
