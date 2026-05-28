namespace MyCarBE.Domain.Enums;

/// <summary>
/// Estado de salud de una cubierta según su profundidad promedio actual.
///
/// Umbrales (referencia industria):
///   ≥ 5mm        → Healthy: cubierta nueva o con poco uso
///   3 – 4.99mm   → Attention: empezar a planificar el cambio
///   1.6 – 2.99mm → ReplaceSoon: cambio en el próximo service
///   &lt; 1.6mm     → Urgent: ilegal circular, cambio inmediato
/// </summary>
public enum TireStatus
{
    Healthy     = 0,
    Attention   = 1,
    ReplaceSoon = 2,
    Urgent      = 3,
}
