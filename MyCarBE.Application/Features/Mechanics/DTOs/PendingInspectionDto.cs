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

    // Km del vehículo registrado al ingreso. Es la línea base del cambio de aceite:
    // el mecánico no lo edita, se hereda de la orden.
    int      MileageAtEntry,

    // Vehículo (para contexto rápido). A propósito NO exponemos propietario/cliente/flota:
    // el mecánico no debe saber para quién es el trabajo (política del taller).
    Guid     VehicleId,
    string   VehicleBrand,
    string   VehicleModel,
    string   VehicleLicensePlate,

    IReadOnlyList<PendingInspectionAreaDto> PendingAreas
);

public record PendingInspectionAreaDto(
    Guid   AreaId,
    string AreaName,
    bool   IsTireArea,
    bool   IsBatteryArea,
    bool   IsOilArea
);
