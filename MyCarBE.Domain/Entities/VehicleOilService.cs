using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Un cambio de aceite y filtros registrado por el mecánico durante la inspección.
/// A diferencia de la batería (un objeto físico con chequeos), cada cambio es un
/// EVENTO discreto: el "actual" es el registro más reciente. No se borra el histórico,
/// así se preserva el track de cuándo se hizo cada service.
///
/// Lleva dos contadores en paralelo desde la base (km/fecha del cambio):
///   - Kilometraje:  NextServiceKm  = ChangedAtKm + IntervalKm   (default +10.000 km)
///   - Tiempo:       NextServiceOn  = ChangedOn.AddMonths(IntervalMonths) (default +6 meses)
/// El que se cumpla primero dispara el aviso. Un nuevo registro reinicia ambos.
/// </summary>
public class VehicleOilService : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    /// <summary>Fecha del cambio de aceite (línea base del contador por tiempo).</summary>
    public DateOnly ChangedOn { get; set; }

    /// <summary>Km del vehículo al momento del cambio (línea base del contador por km).</summary>
    public int ChangedAtKm { get; set; }

    /// <summary>Intervalo en km hasta el próximo service. Editable por el mecánico. Default 10.000.</summary>
    public int IntervalKm { get; set; } = 10000;

    /// <summary>Intervalo en meses hasta el próximo service. Editable por el mecánico. Default 6.</summary>
    public int IntervalMonths { get; set; } = 6;

    /// <summary>Tipo / viscosidad del aceite (ej. "5W30 sintético"). Texto libre, opcional.</summary>
    public string? OilType { get; set; }

    /// <summary>Marca del aceite (ej. "Shell Helix"). Opcional.</summary>
    public string? OilBrand { get; set; }

    /// <summary>Si también se cambió el filtro de aceite en este service.</summary>
    public bool FilterChanged { get; set; } = true;

    public string? Notes { get; set; }

    /// <summary>Usuario que registró el cambio (normalmente el mecánico).</summary>
    public Guid? ChangedByUserId { get; set; }

    /// <summary>
    /// Orden de trabajo (visita) en la que se registró el cambio, si aplica.
    /// Nullable: da trazabilidad sin acoplar el service a la orden.
    /// </summary>
    public Guid? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
}
