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

    /// <summary>
    /// Marca el área responsable del control de cubiertas. El mecánico de esta área
    /// es "el que sabe de cubiertas": al cargar su reporte de inspección puede adjuntar
    /// los datos y mediciones de las cubiertas del vehículo. Solo una área debería tenerlo.
    /// </summary>
    public bool IsTireArea { get; set; }

    /// <summary>
    /// Marca el área responsable del control de batería. El mecánico de esta área, al cargar
    /// su reporte de inspección, puede adjuntar el estado/voltaje de la batería del vehículo.
    /// Solo una área debería tenerlo.
    /// </summary>
    public bool IsBatteryArea { get; set; }

    /// <summary>
    /// Marca el área responsable del control de aceite y filtros. El mecánico de esta área,
    /// al cargar su reporte de inspección, puede registrar el cambio de aceite (km/fecha base
    /// + intervalos) de donde sale la estimación del próximo service. Solo una área debería tenerlo.
    /// </summary>
    public bool IsOilArea { get; set; }

    // Navegación M-a-N — EF Core genera tabla puente automáticamente
    public ICollection<Mechanic> Mechanics { get; set; } = new List<Mechanic>();
}
