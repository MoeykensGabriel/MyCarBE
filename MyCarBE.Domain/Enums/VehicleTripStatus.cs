namespace MyCarBE.Domain.Enums;

/// <summary>
/// Estado de un viaje (vehicle trip) — captura de km al subir/bajar de un vehículo de flota.
/// </summary>
public enum VehicleTripStatus
{
    /// <summary>Viaje abierto: el chofer salió con el auto, todavía no lo devolvió.</summary>
    Open       = 0,

    /// <summary>Viaje cerrado normalmente: el chofer escaneó el QR al volver y registró km de llegada.</summary>
    Closed     = 1,

    /// <summary>
    /// Cerrado automáticamente por el sistema porque otro chofer abrió un viaje nuevo
    /// sin que se haya cerrado el anterior. EndKm queda igual al StartKm del viaje siguiente.
    /// </summary>
    AutoClosed = 2,

    /// <summary>Cerrado a mano por el encargado de la flota desde su panel.</summary>
    ClosedByContact = 3,
}
