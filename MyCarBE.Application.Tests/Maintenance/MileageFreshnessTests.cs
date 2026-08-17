using MyCarBE.Application.Features.Maintenance;
using Xunit;

namespace MyCarBE.Application.Tests.Maintenance;

/// <summary>
/// Tests de <see cref="MileageFreshness"/>: hace cuánto fue la última lectura y si eso ya
/// pasó el umbral del taller. Es lo que le da contexto a la fecha estimada de vencimiento.
/// </summary>
public class MileageFreshnessTests
{
    private static readonly DateTime Now = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private const int Umbral = 14;

    [Fact]
    public void LecturaDeAyer_EstaFresca()
    {
        var f = MileageFreshness.Describe(Now.AddDays(-1), Umbral, Now);

        Assert.Equal(1, f.DaysSince);
        Assert.False(f.IsStale);
    }

    [Fact]
    public void JustoEnElUmbral_YaCuentaComoVencida()
    {
        // El criterio es >=, igual que MileageStaleness en el listado de vehículos. Los dos
        // tienen que decir lo mismo del mismo vehículo el mismo día.
        var f = MileageFreshness.Describe(Now.AddDays(-Umbral), Umbral, Now);

        Assert.Equal(Umbral, f.DaysSince);
        Assert.True(f.IsStale);
    }

    [Fact]
    public void UnDiaAntesDelUmbral_TodaviaEstaFresca()
    {
        var f = MileageFreshness.Describe(Now.AddDays(-(Umbral - 1)), Umbral, Now);

        Assert.Equal(Umbral - 1, f.DaysSince);
        Assert.False(f.IsStale);
    }

    [Fact]
    public void LecturaVieja_EstaVencida()
    {
        var f = MileageFreshness.Describe(Now.AddDays(-120), Umbral, Now);

        Assert.Equal(120, f.DaysSince);
        Assert.True(f.IsStale);
    }

    [Fact]
    public void SinNingunaLectura_CuentaComoVencida()
    {
        // Es donde más falta una lectura, así que no puede quedar como "al día".
        var f = MileageFreshness.Describe(null, Umbral, Now);

        Assert.Null(f.DaysSince);
        Assert.True(f.IsStale);
    }
}
