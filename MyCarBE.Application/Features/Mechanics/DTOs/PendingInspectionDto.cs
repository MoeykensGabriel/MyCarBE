namespace MyCarBE.Application.Features.Mechanics.DTOs;

/// <summary>
/// Una orden en fase de inspección con las áreas pendientes que le corresponden
/// al mecánico logueado. Si una orden tiene 3 áreas pendientes para el mecánico,
/// PendingAreas tendrá 3 elementos.
/// </summary>
public record PendingInspectionDto(
    Guid     WorkOrderId,
    DateTime WorkOrderCreatedAt,
    string?  ServiceReason,

    // Vehículo (para contexto rápido)
    Guid     VehicleId,
    string   VehicleBrand,
    string   VehicleModel,
    string   VehicleLicensePlate,

    // Propietario (cliente individual o flota — texto descriptivo)
    string?  OwnerName,

    IReadOnlyList<PendingInspectionAreaDto> PendingAreas
);

public record PendingInspectionAreaDto(
    Guid   AreaId,
    string AreaName
);
