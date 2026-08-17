using MyCarBE.Application.Features.Maintenance;
using MyCarBE.Domain.Entities;
using Xunit;

namespace MyCarBE.Application.Tests.Maintenance;

/// <summary>
/// Tests de <see cref="MileageRateCalculator"/>: cuántos km por día hace un vehículo según
/// sus lecturas reales de odómetro. Es el insumo para estimar cuándo va a vencer una alerta.
/// Lógica pura, aislada de EF/DB.
/// </summary>
public class MileageRateCalculatorTests
{
    private static VehicleMileageReading Reading(int mileage, DateTime at) =>
        new() { Mileage = mileage, CreatedAt = at };

    private static readonly DateTime Base = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    // ── El caso normal ────────────────────────────────────────────────────────

    [Fact]
    public void DosLecturas_1500KmEn30Dias_Da50KmPorDia()
    {
        var rate = MileageRateCalculator.Calculate(new[]
        {
            Reading(40_000, Base),
            Reading(41_500, Base.AddDays(30)),
        });

        Assert.NotNull(rate);
        Assert.Equal(50m,     rate!.Value.KmPerDay);
        Assert.Equal(30,      rate.Value.DaysSpanned);
        Assert.Equal(1_500,   rate.Value.KmSpanned);
        Assert.Equal(2,       rate.Value.ReadingsUsed);
    }

    [Fact]
    public void UsaLosExtremos_AunqueLasLecturasVenganDesordenadas()
    {
        // El repo devuelve las lecturas "más recientes primero", así que el calculador
        // no puede confiar en el orden en que le llegan.
        var rate = MileageRateCalculator.Calculate(new[]
        {
            Reading(41_500, Base.AddDays(30)),
            Reading(40_000, Base),
            Reading(40_900, Base.AddDays(18)),
        });

        Assert.NotNull(rate);
        Assert.Equal(50m, rate!.Value.KmPerDay);   // extremos: 1.500 km en 30 días
        Assert.Equal(3,   rate.Value.ReadingsUsed); // pero informa que hay 3 lecturas detrás
    }

    // ── Cuándo NO se puede calcular ───────────────────────────────────────────

    [Fact]
    public void UnaSolaLectura_NoHayRitmo()
    {
        var rate = MileageRateCalculator.Calculate(new[] { Reading(40_000, Base) });

        Assert.Null(rate);
    }

    [Fact]
    public void SinLecturas_NoHayRitmo()
    {
        Assert.Null(MileageRateCalculator.Calculate(Array.Empty<VehicleMileageReading>()));
        Assert.Null(MileageRateCalculator.Calculate(null));
    }

    [Fact]
    public void DosLecturasElMismoDia_NoHayRitmo()
    {
        // El caso real: el cliente carga el km y minutos después el taller registra el
        // ingreso. Sin este piso, sería una división por cero.
        var rate = MileageRateCalculator.Calculate(new[]
        {
            Reading(40_000, Base),
            Reading(40_050, Base.AddHours(3)),
        });

        Assert.Null(rate);
    }

    [Fact]
    public void OdometroQueRetrocede_NoProyecta()
    {
        // No debería pasar (el handler lo impide), pero si llega un dato roto preferimos
        // no decir nada antes que devolver una fecha inventada.
        var rate = MileageRateCalculator.Calculate(new[]
        {
            Reading(41_500, Base),
            Reading(40_000, Base.AddDays(30)),
        });

        Assert.Null(rate);
    }

    // ── El auto parado ────────────────────────────────────────────────────────

    [Fact]
    public void AutoQueNoSeMovio_DevuelveRitmoCero_NoNull()
    {
        // Ritmo 0 y "no sé el ritmo" son cosas distintas: acá sabemos que no se movió.
        // Quien estime decide qué hacer (a ritmo 0 no llega nunca al vencimiento).
        var rate = MileageRateCalculator.Calculate(new[]
        {
            Reading(40_000, Base),
            Reading(40_000, Base.AddDays(60)),
        });

        Assert.NotNull(rate);
        Assert.Equal(0m, rate!.Value.KmPerDay);
        Assert.Equal(60, rate.Value.DaysSpanned);
    }

    // ── Redondeo ──────────────────────────────────────────────────────────────

    [Fact]
    public void RitmoConDecimales_RedondeaADosDecimales()
    {
        // 1.000 km en 7 días = 142,857... ⇒ 142,86
        var rate = MileageRateCalculator.Calculate(new[]
        {
            Reading(40_000, Base),
            Reading(41_000, Base.AddDays(7)),
        });

        Assert.NotNull(rate);
        Assert.Equal(142.86m, rate!.Value.KmPerDay);
    }
}
