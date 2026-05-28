namespace MyCarBE.Domain.Enums;

/// <summary>
/// Categoría/calidad del repuesto. Permite agrupar alternativas que el cliente puede elegir
/// (ej: pastilla de freno Genérica vs Original) dentro de un mismo AlternativeGroupId.
/// </summary>
public enum WorkOrderPartTier
{
    Generic     = 0, // Genérico / no-OEM
    Aftermarket = 1, // Aftermarket de marca
    Original    = 2, // Original OEM
    Custom      = 3  // Custom / fuera de catálogo
}
