using MyCarBE.Application.Features.VehicleTires;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;
using Xunit;

namespace MyCarBE.Application.Tests.VehicleTires;

/// <summary>
/// Tests de la lógica de desgaste de cubiertas. Es la parte de mayor riesgo de la feature
/// (regresión lineal + proyecciones), así que la cubrimos exhaustivamente y aislada de EF/DB.
/// </summary>
public class TireWearCalculatorTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static VehicleTire NewTire(
        decimal initialDepthMm = 8.0m,
        int installedAtKm = 0,
        params VehicleTireMeasurement[] measurements)
        => new()
        {
            Id                  = Guid.NewGuid(),
            VehicleId           = Guid.NewGuid(),
            Position            = TirePosition.FrontLeft,
            InstalledOn         = new DateOnly(2026, 1, 1),
            InstalledAtKm       = installedAtKm,
            InitialTreadDepthMm = initialDepthMm,
            IsActive            = true,
            Measurements        = measurements.ToList(),
        };

    private static VehicleTireMeasurement Measurement(
        int km, decimal inner, decimal center, decimal outer, int daysOffset = 0)
        => new()
        {
            Id                          = Guid.NewGuid(),
            MeasuredOn                  = new DateTime(2026, 1, 1).AddDays(daysOffset),
            VehicleMileageAtMeasurement = km,
            InnerDepthMm                = inner,
            CenterDepthMm               = center,
            OuterDepthMm                = outer,
        };

    /// <summary>Atajo: medición uniforme en los 3 puntos.</summary>
    private static VehicleTireMeasurement Even(int km, decimal depth, int daysOffset = 0)
        => Measurement(km, depth, depth, depth, daysOffset);

    // ── Modo 0: sin mediciones ───────────────────────────────────────────────

    [Fact]
    public void NoMeasurements_UsesInitialDepth_NoProjections()
    {
        var tire = NewTire(initialDepthMm: 8.0m, installedAtKm: 0);

        var est = TireWearCalculator.Calculate(tire);

        Assert.Equal(8.0m, est.CurrentAverageDepthMm);
        Assert.Equal(TireStatus.Healthy, est.Status);
        Assert.Null(est.WearRateMmPerKm);
        Assert.Null(est.KmRemainingToReplaceSoon);
        Assert.Null(est.KmRemainingToUrgent);
        Assert.False(est.HasIrregularWear);
        Assert.Null(est.LastMeasuredOn);
        Assert.Null(est.LastMeasurementId);
    }

    // ── Modo 1: una sola medición (tasa simple) ──────────────────────────────

    [Fact]
    public void SingleMeasurement_ComputesSimpleWearRateAndProjections()
    {
        // Nueva 8mm @ km0 → 6mm @ km10.000 ⇒ tasa = 2/10000 = 0.0002 mm/km
        var tire = NewTire(8.0m, 0, Even(10_000, 6.0m));

        var est = TireWearCalculator.Calculate(tire);

        Assert.Equal(6.0m, est.CurrentAverageDepthMm);
        Assert.Equal(TireStatus.Healthy, est.Status); // 6 ≥ 5
        Assert.Equal(0.0002m, est.WearRateMmPerKm);
        Assert.Equal(15_000, est.KmRemainingToReplaceSoon); // (6-3)/0.0002
        Assert.Equal(22_000, est.KmRemainingToUrgent);      // (6-1.6)/0.0002
    }

    [Fact]
    public void SingleMeasurement_AttentionStatus_ProjectsCorrectly()
    {
        // 8mm @ km0 → 4mm @ km20.000 ⇒ tasa 0.0002
        var tire = NewTire(8.0m, 0, Even(20_000, 4.0m));

        var est = TireWearCalculator.Calculate(tire);

        Assert.Equal(TireStatus.Attention, est.Status); // 3 ≤ 4 < 5
        Assert.Equal(5_000, est.KmRemainingToReplaceSoon);  // (4-3)/0.0002
        Assert.Equal(12_000, est.KmRemainingToUrgent);      // (4-1.6)/0.0002
    }

    [Fact]
    public void SingleMeasurement_InstalledAtNonZeroKm_UsesDeltaFromInstall()
    {
        // Instalada con 8mm a los 50.000km, medida 6mm a los 60.000 ⇒ Δkm=10.000, tasa 0.0002
        var tire = NewTire(8.0m, 50_000, Even(60_000, 6.0m));

        var est = TireWearCalculator.Calculate(tire);

        Assert.Equal(0.0002m, est.WearRateMmPerKm);
        Assert.Equal(15_000, est.KmRemainingToReplaceSoon);
    }

    // ── Modo 2: ≥2 mediciones (regresión lineal) ──────────────────────────────

    [Fact]
    public void MultipleMeasurements_UsesLinearRegression()
    {
        // (10k, 6mm), (20k, 4mm) ⇒ pendiente -0.0002 ⇒ tasa 0.0002
        var tire = NewTire(8.0m, 0,
            Even(10_000, 6.0m, daysOffset: 0),
            Even(20_000, 4.0m, daysOffset: 30));

        var est = TireWearCalculator.Calculate(tire);

        Assert.Equal(4.0m, est.CurrentAverageDepthMm); // última medición
        Assert.Equal(TireStatus.Attention, est.Status);
        Assert.Equal(0.0002m, est.WearRateMmPerKm);
        Assert.Equal(5_000, est.KmRemainingToReplaceSoon);
        Assert.Equal(12_000, est.KmRemainingToUrgent);
    }

    [Fact]
    public void MultipleMeasurements_UnorderedInput_StillUsesLatestByMileage()
    {
        // Cargadas fuera de orden: el cálculo debe ordenar por km igual.
        var tire = NewTire(8.0m, 0,
            Even(20_000, 4.0m, daysOffset: 30),
            Even(10_000, 6.0m, daysOffset: 0));

        var est = TireWearCalculator.Calculate(tire);

        Assert.Equal(4.0m, est.CurrentAverageDepthMm); // la de mayor km
        Assert.Equal(0.0002m, est.WearRateMmPerKm);
    }

    [Fact]
    public void MultipleMeasurements_DepthIncreasing_NoProjection()
    {
        // Datos absurdos (la profundidad "sube") ⇒ pendiente ≥ 0 ⇒ no proyectamos.
        var tire = NewTire(8.0m, 0,
            Even(10_000, 4.0m),
            Even(20_000, 6.0m));

        var est = TireWearCalculator.Calculate(tire);

        Assert.Null(est.WearRateMmPerKm);
        Assert.Null(est.KmRemainingToReplaceSoon);
        Assert.Null(est.KmRemainingToUrgent);
    }

    // ── Clasificación de estado ───────────────────────────────────────────────

    [Theory]
    [InlineData(8.0,  TireStatus.Healthy)]
    [InlineData(5.0,  TireStatus.Healthy)]      // límite inferior healthy
    [InlineData(4.99, TireStatus.Attention)]
    [InlineData(3.0,  TireStatus.Attention)]    // límite inferior attention
    [InlineData(2.99, TireStatus.ReplaceSoon)]
    [InlineData(1.6,  TireStatus.ReplaceSoon)]  // límite legal exacto
    [InlineData(1.59, TireStatus.Urgent)]
    [InlineData(0.0,  TireStatus.Urgent)]
    public void ClassifyStatus_RespectsThresholds(double depth, TireStatus expected)
    {
        Assert.Equal(expected, TireWearCalculator.ClassifyStatus((decimal)depth));
    }

    // ── Desgaste irregular ────────────────────────────────────────────────────

    [Fact]
    public void IrregularWear_FlaggedWhenSpreadExceedsThreshold()
    {
        // Inner 8 / Center 8 / Outer 5 ⇒ spread 3 > 2 ⇒ flag. Promedio 7 ⇒ Healthy.
        var tire = NewTire(8.0m, 0, Measurement(10_000, 8.0m, 8.0m, 5.0m));

        var est = TireWearCalculator.Calculate(tire);

        Assert.True(est.HasIrregularWear);
        Assert.Equal(TireStatus.Healthy, est.Status);
    }

    [Fact]
    public void IrregularWear_NotFlaggedWhenSpreadWithinThreshold()
    {
        // spread exactamente 2.0 ⇒ NO dispara (umbral es estrictamente > 2).
        var tire = NewTire(8.0m, 0, Measurement(10_000, 7.0m, 6.0m, 5.0m));

        var est = TireWearCalculator.Calculate(tire);

        Assert.False(est.HasIrregularWear);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public void AlreadyBelowThreshold_ReturnsZeroKmRemaining()
    {
        // 8mm @ km0 → 2.5mm @ km30.000 (ReplaceSoon). Ya está por debajo de 3mm.
        var tire = NewTire(8.0m, 0, Even(30_000, 2.5m));

        var est = TireWearCalculator.Calculate(tire);

        Assert.Equal(TireStatus.ReplaceSoon, est.Status);
        Assert.Equal(0, est.KmRemainingToReplaceSoon); // ya pasó el umbral de 3mm
        Assert.True(est.KmRemainingToUrgent > 0);      // todavía no llega a 1.6mm
    }

    [Fact]
    public void NoWearDetected_DepthEqualsInitial_NoProjection()
    {
        // Medición da la misma profundidad inicial ⇒ Δdepth = 0 ⇒ no podemos proyectar.
        var tire = NewTire(8.0m, 0, Even(10_000, 8.0m));

        var est = TireWearCalculator.Calculate(tire);

        Assert.Null(est.WearRateMmPerKm);
        Assert.Null(est.KmRemainingToReplaceSoon);
    }

    [Fact]
    public void MeasurementBeforeInstall_InconsistentData_NoProjection()
    {
        // Δkm negativo (medición con km menor al de instalación) ⇒ no proyectamos.
        var tire = NewTire(8.0m, 10_000, Even(5_000, 6.0m));

        var est = TireWearCalculator.Calculate(tire);

        Assert.Null(est.WearRateMmPerKm);
    }

    [Fact]
    public void Estimation_ExposesLastMeasurementMetadata()
    {
        var last = Even(20_000, 4.0m, daysOffset: 30);
        var tire = NewTire(8.0m, 0, Even(10_000, 6.0m, daysOffset: 0), last);

        var est = TireWearCalculator.Calculate(tire);

        Assert.Equal(last.Id, est.LastMeasurementId);
        Assert.Equal(last.MeasuredOn, est.LastMeasuredOn);
    }
}
