using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Una lectura puntual del odómetro del vehículo. Es un log de solo-agregar:
/// cada fuente (ingreso al taller, declaración del cliente, etc.) suma una fila
/// y NUNCA se editan las anteriores — eso da la trazabilidad del kilometraje
/// a lo largo del tiempo (ritmo de uso, proyecciones de service).
///
/// Vehicle.CurrentMileage y Vehicle.MileageUpdatedAt son el cache de la última
/// lectura: se actualizan al agregar una lectura nueva.
///
/// La fecha de la lectura es CreatedAt (BaseEntity): las lecturas se registran
/// en el momento, no retroactivamente.
/// </summary>
public class VehicleMileageReading : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    /// <summary>Kilometraje leído del odómetro.</summary>
    public int Mileage { get; set; }

    public MileageReadingSource Source { get; set; }

    /// <summary>Usuario que registró la lectura (cliente, admin u oficina).</summary>
    public Guid? ReportedByUserId { get; set; }

    /// <summary>
    /// Orden de trabajo asociada cuando la fuente es el ingreso al taller.
    /// Da trazabilidad sin acoplar la lectura a la orden.
    /// </summary>
    public Guid? WorkOrderId { get; set; }
}
