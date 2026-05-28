using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Una cubierta (neumático) instalada en una posición específica del vehículo.
/// Cuando se reemplaza, NO se borra — se marca IsActive=false y queda en historial.
/// Esto preserva el track de cuántos km duró cada cubierta del cliente.
///
/// Constraint lógico: un vehículo solo puede tener UNA cubierta activa por (VehicleId, Position).
/// </summary>
public class VehicleTire : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public TirePosition Position { get; set; }

    /// <summary>Marca: Pirelli, Michelin, Bridgestone, etc.</summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>Modelo / línea: P7, Cinturato, etc.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Medida: "185/65 R15" (texto libre — formatos varían entre fabricantes).</summary>
    public string SizeSpec { get; set; } = string.Empty;

    public DateOnly InstalledOn { get; set; }
    public int InstalledAtKm { get; set; }

    /// <summary>
    /// Profundidad inicial de la banda de rodamiento (mm). Default técnico: 8mm.
    /// Algunas cubiertas vienen con 7 u 9 según fabricante — la oficina la carga.
    /// </summary>
    public decimal InitialTreadDepthMm { get; set; } = 8.0m;

    /// <summary>
    /// Vida útil esperada según el fabricante en km (ej. 50.000). Sirve para estimación
    /// temprana cuando todavía no hay mediciones reales.
    /// </summary>
    public int ExpectedLifeKm { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Fecha en que se reemplazó la cubierta. Null mientras esté activa.</summary>
    public DateOnly? ReplacedOn { get; set; }

    /// <summary>Km del vehículo al momento del reemplazo. Null mientras esté activa.</summary>
    public int? ReplacedAtKm { get; set; }

    /// <summary>Mediciones de profundidad a lo largo del tiempo (ordenadas por fecha).</summary>
    public ICollection<VehicleTireMeasurement> Measurements { get; set; } = new List<VehicleTireMeasurement>();
}
