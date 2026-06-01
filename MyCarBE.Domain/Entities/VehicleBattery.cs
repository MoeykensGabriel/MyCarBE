using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// La batería instalada en un vehículo. A diferencia de las cubiertas (4 posiciones),
/// el vehículo tiene UNA batería activa. Cuando se reemplaza, NO se borra — se marca
/// IsActive=false y queda en historial, para preservar el track de duración.
///
/// Constraint lógico: un vehículo solo puede tener UNA batería activa.
/// </summary>
public class VehicleBattery : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    /// <summary>Marca de la batería (Bosch, Moura, etc.). Opcional.</summary>
    public string? Brand { get; set; }

    /// <summary>Fecha de fabricación de la batería, si se conoce. Sirve para estimar antigüedad.</summary>
    public DateOnly? ManufacturedOn { get; set; }

    public DateOnly InstalledOn { get; set; }
    public int InstalledAtKm { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Fecha en que se reemplazó la batería. Null mientras esté activa.</summary>
    public DateOnly? ReplacedOn { get; set; }

    /// <summary>Km del vehículo al momento del reemplazo. Null mientras esté activa.</summary>
    public int? ReplacedAtKm { get; set; }

    /// <summary>Chequeos de estado a lo largo del tiempo (ordenados por fecha).</summary>
    public ICollection<VehicleBatteryCheck> Checks { get; set; } = new List<VehicleBatteryCheck>();
}
