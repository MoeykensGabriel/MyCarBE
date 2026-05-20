using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Configuración global del taller editable por el admin. Es un "singleton":
/// existe exactamente una fila en esta tabla.
///
/// Más adelante se le pueden agregar columnas (horarios, contacto, etc.) sin
/// cambiar el patrón.
/// </summary>
public class WorkshopSettings : BaseEntity
{
    /// <summary>
    /// Cantidad de vehículos que el taller puede albergar simultáneamente.
    /// Usado en el dashboard para mostrar "X / Y lugares ocupados".
    /// </summary>
    public int PhysicalCapacity { get; set; } = 6;
}
