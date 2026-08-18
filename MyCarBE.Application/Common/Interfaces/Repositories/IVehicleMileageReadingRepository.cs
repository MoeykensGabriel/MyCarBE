using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

/// <summary>
/// Los dos extremos del historial de lecturas de un vehículo. Es todo lo que necesita el
/// cálculo del ritmo de uso: cuánto marcaba y cuándo, la primera vez y la última.
/// <paramref name="ReadingsCount"/> no entra en la cuenta pero sí en la confianza.
/// </summary>
public record VehicleMileageSpan(
    Guid     VehicleId,
    int      FirstMileage,
    DateTime FirstAt,
    int      LastMileage,
    DateTime LastAt,
    int      ReadingsCount);

public interface IVehicleMileageReadingRepository : IRepository<VehicleMileageReading>
{
    /// <summary>
    /// Últimas lecturas del vehículo, más recientes primero. <paramref name="take"/>
    /// acota el histórico (la trazabilidad típica son las últimas semanas, no años).
    /// </summary>
    Task<IReadOnlyList<VehicleMileageReading>> GetLatestByVehicleAsync(
        Guid vehicleId, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extremos del historial de VARIOS vehículos en una sola consulta.
    ///
    /// Existe por el Inicio del cliente, que evalúa las alertas de todos sus vehículos de una
    /// pasada: pedir las lecturas de a un vehículo ahí sería un N+1. Los vehículos sin
    /// lecturas simplemente no aparecen en el resultado.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, VehicleMileageSpan>> GetSpansByVehiclesAsync(
        IReadOnlyCollection<Guid> vehicleIds, CancellationToken cancellationToken = default);
}
