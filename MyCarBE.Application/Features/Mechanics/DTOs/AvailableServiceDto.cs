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
    // Sin precio a proposito, por la misma politica que el propietario de abajo: el
    // mecanico no ve plata. Elige que trabajo tomar por el trabajo en si, no por lo que
    // factura. Y si viajara igual —aunque la pantalla no lo dibuje— alcanzaria con abrir
    // las herramientas del navegador para verlo.
    int      EstimatedDurationMinutes,

    // Cuándo entró al pool (timestamp del servicio, no de la WO)
    DateTime CreatedAt,

    // Vehículo (para contexto rápido). A propósito NO exponemos propietario/cliente/flota:
    // el mecánico no debe saber para quién es el trabajo (política del taller).
    Guid     VehicleId,
    string   VehicleBrand,
    string   VehicleModel,
    string   VehicleLicensePlate
);
