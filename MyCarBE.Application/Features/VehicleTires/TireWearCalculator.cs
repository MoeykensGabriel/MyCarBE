using MyCarBE.Application.Features.VehicleTires.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleTires;

/// <summary>
/// Núcleo del dominio de cubiertas: dada una cubierta + su historial de mediciones,
/// devuelve estado actual, tasa de desgaste y km estimados a los umbrales de 3mm y 1.6mm.
///
/// Umbrales (fuente: industria):
///   3mm   = planificar cambio (atención)
///   1.6mm = cambio urgente / ilegal
///
/// Dos modos según cuánta info hay:
///   - 0 mediciones: no se puede estimar nada, solo se informa el estado por la profundidad inicial.
///   - 1 medición : tasa = (Inicial − Actual) / (KmActual − KmInstalado).
///   - ≥ 2 mediciones: regresión lineal simple (mínimos cuadrados) sobre (km, profundidad).
///                     Captura la tasa REAL de uso del cliente — más confiable que la teórica.
///
/// Toda la matemática vive acá (pura, sin EF / sin DB) para poder testearla aislada.
/// </summary>
public static class TireWearCalculator
{
    public const decimal ThresholdAttentionMm   = 5.0m;
    public const decimal ThresholdReplaceSoonMm = 3.0m;
    public const decimal ThresholdUrgentMm      = 1.6m;

    /// <summary>Diferencia max-min entre los 3 puntos que dispara el flag de desgaste irregular.</summary>
    public const decimal IrregularSpreadThresholdMm = 2.0m;

    public static TireWearEstimationDto Calculate(VehicleTire tire)
    {
        var measurements = (tire.Measurements ?? new List<VehicleTireMeasurement>())
            .OrderBy(m => m.VehicleMileageAtMeasurement)
            .ToList();

        // Profundidad actual: SIEMPRE el último valor medido por el técnico de gomería
        // en la inspección (dato real, nunca estimado). Si todavía no hay mediciones,
        // usamos la profundidad inicial de fábrica como referencia.
        var last           = measurements.LastOrDefault();
        var currentDepthMm = last?.AverageDepthMm ?? tire.InitialTreadDepthMm;

        var status = ClassifyStatus(currentDepthMm);

        // Tasa de desgaste (mm por km). null = no se puede calcular todavía.
        decimal? wearRateMmPerKm = ComputeWearRate(tire, measurements);

        int? kmRemainingTo3mm  = ProjectKmRemaining(currentDepthMm, ThresholdReplaceSoonMm, wearRateMmPerKm);
        int? kmRemainingTo16mm = ProjectKmRemaining(currentDepthMm, ThresholdUrgentMm,      wearRateMmPerKm);

        // Desgaste irregular: lo dispara la ÚLTIMA medición. Para historial podríamos mirar todas,
        // pero el aviso al cliente es accionable solo sobre el estado actual.
        var hasIrregularWear = last is not null && last.SpreadMm > IrregularSpreadThresholdMm;

        return new TireWearEstimationDto(
            CurrentAverageDepthMm:    decimal.Round(currentDepthMm,    2),
            Status:                   status,
            WearRateMmPerKm:          wearRateMmPerKm.HasValue ? decimal.Round(wearRateMmPerKm.Value, 6) : null,
            KmRemainingToReplaceSoon: kmRemainingTo3mm,
            KmRemainingToUrgent:      kmRemainingTo16mm,
            HasIrregularWear:         hasIrregularWear,
            LastMeasuredOn:           last?.MeasuredOn,
            LastMeasurementId:        last?.Id
        );
    }

    public static TireStatus ClassifyStatus(decimal depthMm) => depthMm switch
    {
        >= ThresholdAttentionMm   => TireStatus.Healthy,
        >= ThresholdReplaceSoonMm => TireStatus.Attention,
        >= ThresholdUrgentMm      => TireStatus.ReplaceSoon,
        _                         => TireStatus.Urgent,
    };

    private static decimal? ComputeWearRate(VehicleTire tire, List<VehicleTireMeasurement> measurements)
    {
        // Caso 0: sin mediciones — no hay forma de medir cuánto desgastó.
        if (measurements.Count == 0) return null;

        // Caso 1: una sola medición — tasa simple contra el punto de instalación.
        if (measurements.Count == 1)
        {
            var m       = measurements[0];
            var deltaKm = m.VehicleMileageAtMeasurement - tire.InstalledAtKm;
            if (deltaKm <= 0) return null; // datos inconsistentes (medido antes de instalar) → no proyectamos

            var deltaDepth = tire.InitialTreadDepthMm - m.AverageDepthMm;
            if (deltaDepth <= 0) return null; // no se desgastó (o subió) → no podemos proyectar

            return deltaDepth / deltaKm;
        }

        // Caso 2+: regresión lineal por mínimos cuadrados sobre (km, depth).
        // pendiente = (n·Σxy − Σx·Σy) / (n·Σx² − (Σx)²)
        // La pendiente es NEGATIVA (la profundidad baja al subir km) → la negamos para devolver mm/km positivos.
        int     n     = measurements.Count;
        decimal sumX  = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        foreach (var m in measurements)
        {
            decimal x = m.VehicleMileageAtMeasurement;
            decimal y = m.AverageDepthMm;
            sumX  += x;
            sumY  += y;
            sumXY += x * y;
            sumX2 += x * x;
        }
        var denom = n * sumX2 - sumX * sumX;
        if (denom == 0) return null;

        var slope = (n * sumXY - sumX * sumY) / denom;
        if (slope >= 0) return null; // no hay desgaste detectable → no proyectamos

        return -slope;
    }

    private static int? ProjectKmRemaining(decimal currentDepthMm, decimal targetDepthMm, decimal? wearRateMmPerKm)
    {
        if (!wearRateMmPerKm.HasValue || wearRateMmPerKm.Value <= 0) return null;
        if (currentDepthMm <= targetDepthMm) return 0; // ya está debajo del umbral

        var remaining = (currentDepthMm - targetDepthMm) / wearRateMmPerKm.Value;
        if (remaining < 0) return 0;
        if (remaining > int.MaxValue) return int.MaxValue;

        return (int)Math.Floor((double)remaining);
    }
}
