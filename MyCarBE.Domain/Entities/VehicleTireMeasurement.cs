using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Una medición puntual de profundidad de banda de rodamiento de UNA cubierta.
/// Tomada con medidor en 3 puntos para detectar desgaste irregular:
///   - Inner  (lado interior, cerca del chasis)
///   - Center (centro de la banda)
///   - Outer  (lado exterior, hacia afuera)
///
/// El promedio define el estado de la cubierta. La diferencia entre los 3 puntos
/// indica si hay desgaste irregular (problema de alineación / inflado / amortiguación).
/// </summary>
public class VehicleTireMeasurement : BaseEntity
{
    public Guid VehicleTireId { get; set; }
    public VehicleTire VehicleTire { get; set; } = null!;

    public DateTime MeasuredOn { get; set; }

    /// <summary>Km del vehículo al momento de la medición (clave para calcular tasa de desgaste).</summary>
    public int VehicleMileageAtMeasurement { get; set; }

    public decimal InnerDepthMm  { get; set; }
    public decimal CenterDepthMm { get; set; }
    public decimal OuterDepthMm  { get; set; }

    public string? Notes { get; set; }

    /// <summary>Usuario que tomó la medición (admin, recepcionista o mecánico).</summary>
    public Guid? MeasuredByUserId { get; set; }

    /// <summary>Promedio de los tres puntos. Útil para queries; no se persiste — se calcula al vuelo.</summary>
    public decimal AverageDepthMm => (InnerDepthMm + CenterDepthMm + OuterDepthMm) / 3m;

    /// <summary>
    /// Spread entre el punto más alto y el más bajo. Si supera ~2mm, hay desgaste irregular
    /// y conviene avisar al cliente.
    /// </summary>
    public decimal SpreadMm
    {
        get
        {
            var max = Math.Max(InnerDepthMm, Math.Max(CenterDepthMm, OuterDepthMm));
            var min = Math.Min(InnerDepthMm, Math.Min(CenterDepthMm, OuterDepthMm));
            return max - min;
        }
    }
}
