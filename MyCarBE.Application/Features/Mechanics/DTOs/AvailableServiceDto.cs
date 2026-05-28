namespace MyCarBE.Application.Features.Mechanics.DTOs;

/// <summary>
/// Un servicio del pool de trabajos disponibles para el mecánico.
/// Shape liviano — la pantalla del pool no necesita la WO completa.
/// </summary>
public record AvailableServiceDto(
    Guid     WorkOrderServiceId,
    Guid     WorkOrderId,

    // Datos del servicio
    string   ServiceName,
    string?  ServiceDescription,
    int      Quantity,
    decimal  PriceSnapshot,
    int      EstimatedDurationMinutes,

    // Cuándo entró al pool (timestamp del servicio, no de la WO)
    DateTime CreatedAt,

    // Vehículo (para contexto rápido)
    Guid     VehicleId,
    string   VehicleBrand,
    string   VehicleModel,
    string   VehicleLicensePlate,

    // Propietario (cliente individual o flota — texto descriptivo)
    string?  OwnerName
);
