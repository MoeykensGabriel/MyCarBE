using MyCarBE.Application.Features.Maintenance;
using MyCarBE.Domain.Enums;
using Xunit;

namespace MyCarBE.Application.Tests.Maintenance;

/// <summary>
/// Tests de <see cref="MaintenanceAlertMappings.ResolveBaselineMileage"/>: la línea base de km
/// con la que arranca una alerta NUEVA. Es el "factor de corrección" para autos que entran al
/// taller con km ya recorridos — el caso que planteó el jefe (auto que entra a 40.000 con
/// intervalo 60.000 debe avisar a 60.000, no a 100.000). Lógica pura, aislada de EF/DB.
/// </summary>
public class ResolveBaselineMileageTests
{
    // ── El caso del jefe: ítems "desde fábrica" ──────────────────────────────

    [Fact]
    public void FactoryMilestone_EntersMidCycle_AlignsToFactoryMultiple()
    {
        // Auto que entra a 40.000 con transmisión cada 60.000, sin saber el último cambio.
        // Línea base = múltiplo de fábrica por abajo = 0 ⇒ próximo aviso a 60.000 (faltan 20.000).
        var baseline = MaintenanceAlertMappings.ResolveBaselineMileage(
            MaintenanceItemType.Transmission, intervalKm: 60_000, currentMileage: 40_000,
            lastServiceMileage: null);

        Assert.Equal(0, baseline);
        Assert.Equal(60_000, baseline + 60_000); // próximo hito real
    }

    [Fact]
    public void FactoryMilestone_PastFirstMilestone_AlignsToNearestLowerMultiple()
    {
        // Entra a 130.000 con kit de distribución cada 60.000 ⇒ base 120.000, avisa a 180.000.
        var baseline = MaintenanceAlertMappings.ResolveBaselineMileage(
            MaintenanceItemType.TimingKit, intervalKm: 60_000, currentMileage: 130_000,
            lastServiceMileage: null);

        Assert.Equal(120_000, baseline);
    }

    [Fact]
    public void FactoryMilestone_ExactlyOnMultiple_KeepsThatMultiple()
    {
        // Entra justo a 120.000 ⇒ base 120.000 (no 60.000): la división entera ya hace el floor.
        var baseline = MaintenanceAlertMappings.ResolveBaselineMileage(
            MaintenanceItemType.Differential, intervalKm: 60_000, currentMileage: 120_000,
            lastServiceMileage: null);

        Assert.Equal(120_000, baseline);
    }

    // ── Consumibles: el ciclo arranca en el ingreso ──────────────────────────

    [Fact]
    public void Consumable_StartsCycleAtEntryMileage()
    {
        // Aceite cada 10.000, auto que entra a 40.000 ⇒ base 40.000 (avisa a 50.000), NO se
        // alinea a fábrica: no sabemos cuándo se cambió, el ingreso es el punto de partida.
        var baseline = MaintenanceAlertMappings.ResolveBaselineMileage(
            MaintenanceItemType.Oil, intervalKm: 10_000, currentMileage: 40_000,
            lastServiceMileage: null);

        Assert.Equal(40_000, baseline);
    }

    // ── El override del recepcionista pisa todo ───────────────────────────────

    [Fact]
    public void LastServiceMileage_OverridesEverything_ForConsumable()
    {
        // Si el recepcionista carga el último cambio, manda eso aunque sea consumible.
        var baseline = MaintenanceAlertMappings.ResolveBaselineMileage(
            MaintenanceItemType.Oil, intervalKm: 10_000, currentMileage: 40_000,
            lastServiceMileage: 35_000);

        Assert.Equal(35_000, baseline);
    }

    [Fact]
    public void LastServiceMileage_OverridesFactoryAlignment()
    {
        // El override gana incluso sobre la alineación de fábrica: si dijo 55.000, es 55.000.
        var baseline = MaintenanceAlertMappings.ResolveBaselineMileage(
            MaintenanceItemType.Transmission, intervalKm: 60_000, currentMileage: 40_000,
            lastServiceMileage: 55_000);

        Assert.Equal(55_000, baseline);
    }

    // ── Sin intervalo de km (alerta solo por tiempo) ─────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void FactoryMilestone_WithoutKmInterval_FallsBackToCurrentMileage(int? intervalKm)
    {
        // Sin km de intervalo no hay múltiplo de fábrica que calcular ⇒ cae al km actual.
        var baseline = MaintenanceAlertMappings.ResolveBaselineMileage(
            MaintenanceItemType.Transmission, intervalKm, currentMileage: 40_000,
            lastServiceMileage: null);

        Assert.Equal(40_000, baseline);
    }

    // ── Clasificación desde-fábrica vs consumible ────────────────────────────

    [Theory]
    [InlineData(MaintenanceItemType.TimingKit,    true)]
    [InlineData(MaintenanceItemType.Transmission, true)]
    [InlineData(MaintenanceItemType.Differential, true)]
    [InlineData(MaintenanceItemType.SparkPlugs,   true)]
    [InlineData(MaintenanceItemType.Oil,          false)]
    [InlineData(MaintenanceItemType.Tires,        false)]
    [InlineData(MaintenanceItemType.Battery,      false)]
    [InlineData(MaintenanceItemType.InjectorCleaning, false)]
    [InlineData(MaintenanceItemType.Other,        false)]
    public void IsFactoryMilestone_ClassifiesEachType(MaintenanceItemType type, bool expected)
    {
        Assert.Equal(expected, MaintenanceAlertMappings.IsFactoryMilestone(type));
    }
}
