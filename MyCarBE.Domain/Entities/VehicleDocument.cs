using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Documento de un vehículo con fecha de vencimiento (VTV, póliza, patente, etc.).
/// Lo carga el cliente (o el taller en su nombre) y el sistema alerta cuando se acerca
/// la fecha de vencimiento.
///
/// No se modela como histórico — cada vez que se renueva, se actualiza ExpiresOn
/// in-place. Si en el futuro se quiere historial, se hace en otra entidad.
/// </summary>
public class VehicleDocument : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public VehicleDocumentType DocumentType { get; set; }

    /// <summary>Fecha de vencimiento (solo fecha, sin hora).</summary>
    public DateOnly ExpiresOn { get; set; }

    /// <summary>
    /// Notas libres: nº de póliza, aseguradora, observaciones. Obligatorio si DocumentType=Other
    /// (para saber qué documento es).
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>Quién emitió el documento (ej: "La Caja", "DNRPA"). Opcional.</summary>
    public string? IssuingEntity { get; set; }
}
