using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Un chequeo puntual del estado de la batería, tomado por el mecánico durante una inspección.
/// El estado lo define el mecánico (no se calcula). El voltaje en reposo es un dato técnico
/// opcional. Cada chequeo queda trazado a la orden de trabajo en que se hizo.
/// </summary>
public class VehicleBatteryCheck : BaseEntity
{
    public Guid VehicleBatteryId { get; set; }
    public VehicleBattery VehicleBattery { get; set; } = null!;

    public DateTime CheckedOn { get; set; }

    /// <summary>Km del vehículo al momento del chequeo.</summary>
    public int VehicleMileageAtCheck { get; set; }

    /// <summary>Estado reportado por el mecánico.</summary>
    public BatteryStatus Status { get; set; }

    /// <summary>Voltaje en reposo (V), opcional. Ej. 12.6.</summary>
    public decimal? Voltage { get; set; }

    public string? Notes { get; set; }

    /// <summary>Usuario que hizo el chequeo (normalmente el mecánico).</summary>
    public Guid? CheckedByUserId { get; set; }

    /// <summary>
    /// Orden de trabajo (visita) en la que se hizo el chequeo, si aplica.
    /// Nullable: da trazabilidad sin acoplar la batería a la orden.
    /// </summary>
    public Guid? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
}
