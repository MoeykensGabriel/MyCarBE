using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Viaje de un vehículo de flota: el chofer escanea el QR pegado en el auto, carga su nombre
/// y los km de salida. Cuando vuelve, escanea de nuevo y carga los km de llegada.
///
/// El chofer NO tiene cuenta en el sistema — la identificación es texto libre (nombre + DNI).
/// La validación de "quién puede registrar" la da la posesión del QR, que vive físicamente
/// en el vehículo (sticker en parabrisas / guantera).
/// </summary>
public class VehicleTrip : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    /// <summary>Nombre del chofer tal como lo tipeó. No se valida.</summary>
    public string DriverName { get; set; } = string.Empty;

    /// <summary>DNI / documento del chofer. Sirve como clave de agrupación en reportes.</summary>
    public string DriverDocument { get; set; } = string.Empty;

    public int StartKm { get; set; }
    public int? EndKm { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public VehicleTripStatus Status { get; set; } = VehicleTripStatus.Open;
}
