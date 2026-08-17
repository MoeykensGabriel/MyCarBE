using MyCarBE.Application.Features.Maintenance;
using MyCarBE.Application.Features.Maintenance.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;
using Xunit;

namespace MyCarBE.Application.Tests.Maintenance;

/// <summary>
/// Tests de la estimación de vencimiento en <see cref="MaintenanceAlertStatusCalculator"/>:
/// los km que faltan traducidos a una fecha usando el ritmo de uso del vehículo.
///
/// Lo que se prueba acá es que la estimación INFORMA y no manda: no toca la severidad, y
/// la fecha dura del contador de tiempo sigue viajando aparte de la estimada.
/// </summary>
public class AlertEstimatedDueDateTests
{
    private static readonly DateTime Now = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private static MaintenanceAlert AlertaPorKm(int intervalKm, int baselineMileage) =>
        new()
        {
            ItemType        = MaintenanceItemType.Oil,
            IntervalKm      = intervalKm,
            BaselineMileage = baselineMileage,
            BaselineDate    = Now.AddMonths(-6),
        };

    private static MileageRateCalculator.MileageRate Ritmo(decimal kmPorDia) =>
        new(KmPerDay: kmPorDia, DaysSpanned: 90, KmSpanned: (int)(kmPorDia * 90), ReadingsUsed: 5);

    // ── El caso central ───────────────────────────────────────────────────────

    [Fact]
    public void FaltanMilKm_ARitmoDe50PorDia_Estima20Dias()
    {
        // Alerta cada 10.000 km desde 30.000 ⇒ vence a 40.000. El auto está en 39.000.
        var alert = AlertaPorKm(intervalKm: 10_000, baselineMileage: 30_000);

        var e = MaintenanceAlertStatusCalculator.Evaluate(
            alert, currentMileage: 39_000, now: Now, severityFloor: null, rate: Ritmo(50m));

        Assert.Equal(1_000, e.KmRemaining);
        Assert.Equal(20,    e.EstimatedDaysFromKm);
        Assert.Equal(Now.AddDays(20), e.EstimatedDueDate);
    }

    [Fact]
    public void DiasEstimados_RedondeanParaArriba()
    {
        // Faltan 1.000 km a 30 km/día = 33,33 días ⇒ 34. Nunca prometer de menos.
        var alert = AlertaPorKm(intervalKm: 10_000, baselineMileage: 30_000);

        var e = MaintenanceAlertStatusCalculator.Evaluate(
            alert, currentMileage: 39_000, now: Now, severityFloor: null, rate: Ritmo(30m));

        Assert.Equal(34, e.EstimatedDaysFromKm);
    }

    // ── La regresión que más importa ──────────────────────────────────────────

    [Fact]
    public void SinRitmo_SeComportaIgualQueAntes()
    {
        // Los llamadores que todavía no resuelven el ritmo no cambian en nada.
        var alert = AlertaPorKm(intervalKm: 10_000, baselineMileage: 30_000);

        var e = MaintenanceAlertStatusCalculator.Evaluate(alert, currentMileage: 39_000, now: Now);

        Assert.Equal(1_000, e.KmRemaining);
        Assert.Null(e.EstimatedDaysFromKm);
        Assert.Null(e.EstimatedDueDate);
    }

    // ── Cuándo no se estima ───────────────────────────────────────────────────

    [Fact]
    public void AutoParado_NoEstima()
    {
        // Ritmo 0: a ese ritmo no llega nunca al vencimiento.
        var alert = AlertaPorKm(intervalKm: 10_000, baselineMileage: 30_000);

        var e = MaintenanceAlertStatusCalculator.Evaluate(
            alert, currentMileage: 39_000, now: Now, severityFloor: null, rate: Ritmo(0m));

        Assert.Null(e.EstimatedDaysFromKm);
        Assert.Null(e.EstimatedDueDate);
    }

    [Fact]
    public void AlertaYaVencidaPorKm_NoEstimaFechaEnElPasado()
    {
        // Vence a 40.000 y el auto va por 41.500. Que está vencida ya lo dicen la severidad
        // y los km en negativo; una fecha pasada solo agregaría ruido.
        var alert = AlertaPorKm(intervalKm: 10_000, baselineMileage: 30_000);

        var e = MaintenanceAlertStatusCalculator.Evaluate(
            alert, currentMileage: 41_500, now: Now, severityFloor: null, rate: Ritmo(50m));

        Assert.Equal(-1_500, e.KmRemaining);
        Assert.Equal(MaintenanceAlertSeverity.Critical, e.Severity);
        Assert.Null(e.EstimatedDaysFromKm);
        Assert.Null(e.EstimatedDueDate);
    }

    [Fact]
    public void AlertaSoloPorTiempo_NoTieneQueEstimar()
    {
        // Sin IntervalKm no hay km que traducir, por más ritmo que haya.
        var alert = new MaintenanceAlert
        {
            ItemType       = MaintenanceItemType.Oil,
            IntervalMonths = 12,
            BaselineDate   = Now.AddMonths(-6),
        };

        var e = MaintenanceAlertStatusCalculator.Evaluate(
            alert, currentMileage: 39_000, now: Now, severityFloor: null, rate: Ritmo(50m));

        Assert.Null(e.KmRemaining);
        Assert.Null(e.EstimatedDaysFromKm);
        Assert.NotNull(e.DaysRemaining);   // el contador de tiempo sigue andando
    }

    // ── Km y tiempo conviviendo ───────────────────────────────────────────────

    [Fact]
    public void ConKmYTiempo_LasDosFechasViajanPorSeparado()
    {
        // Aceite: cada 10.000 km O cada 12 meses. Faltan 1.000 km (20 días al ritmo actual),
        // pero por calendario recién vence dentro de 6 meses. La dura y la blanda no se mezclan.
        var alert = new MaintenanceAlert
        {
            ItemType        = MaintenanceItemType.Oil,
            IntervalKm      = 10_000,
            IntervalMonths  = 12,
            BaselineMileage = 30_000,
            BaselineDate    = Now.AddMonths(-6),
        };

        var e = MaintenanceAlertStatusCalculator.Evaluate(
            alert, currentMileage: 39_000, now: Now, severityFloor: null, rate: Ritmo(50m));

        Assert.Equal(20, e.EstimatedDaysFromKm);          // blanda: por km
        Assert.True(e.DaysRemaining > 150);               // dura: por calendario, mucho después
        Assert.NotEqual(e.DaysRemaining, e.EstimatedDaysFromKm);
    }

    // ── La estimación no manda ────────────────────────────────────────────────

    [Fact]
    public void EstimacionCercana_NoMueveLaSeveridad()
    {
        // Faltan 5.000 km: lejos del umbral de 1.000 km, así que la alerta está en Ok. Que
        // el ritmo diga "en 100 días" no la escala — informa, no dispara.
        var alert = AlertaPorKm(intervalKm: 10_000, baselineMileage: 30_000);

        var e = MaintenanceAlertStatusCalculator.Evaluate(
            alert, currentMileage: 35_000, now: Now, severityFloor: null, rate: Ritmo(50m));

        Assert.Null(e.Severity);                 // sigue en Ok
        Assert.Equal(100, e.EstimatedDaysFromKm);
    }
}
