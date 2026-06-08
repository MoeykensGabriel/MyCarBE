using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

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

    // ─── Specs físicas del repuesto (opcionales) ──────────────────────────────
    // Identifican qué batería comprar: dos de igual capacidad pueden venir en
    // cajas o con bornes distintos según el vehículo.

    /// <summary>Capacidad en amperios-hora (Ah). Ej. 75. Opcional.</summary>
    public int? CapacityAh { get; set; }

    /// <summary>Ancho de la caja en cm (mirando de frente). Opcional.</summary>
    public int? BoxWidthCm { get; set; }

    /// <summary>Largo / profundidad de la caja en cm. Opcional.</summary>
    public int? BoxLengthCm { get; set; }

    /// <summary>Alto de la caja en cm. Opcional.</summary>
    public int? BoxHeightCm { get; set; }

    /// <summary>Lado del borne positivo (+) mirando la batería de frente. Opcional.</summary>
    public BatteryTerminalSide? PositiveTerminalSide { get; set; }

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
