using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Área de especialidad del taller (Motor, Frenos, Tren delantero, etc.).
/// Un mecánico puede pertenecer a varias áreas (M-a-N) y una orden, durante la
/// fase de inspección colectiva, espera un reporte por cada área aplicable.
/// </summary>
public class Area : BaseEntity
{
    public string Name { get; set; } = string.Empty; // único
    public bool IsActive { get; set; } = true;

    // Navegación M-a-N — EF Core genera tabla puente automáticamente
    public ICollection<Mechanic> Mechanics { get; set; } = new List<Mechanic>();
}
