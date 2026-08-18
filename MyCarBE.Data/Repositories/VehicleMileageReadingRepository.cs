using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class VehicleMileageReadingRepository : Repository<VehicleMileageReading>, IVehicleMileageReadingRepository
{
    public VehicleMileageReadingRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<VehicleMileageReading>> GetLatestByVehicleAsync(
        Guid vehicleId, int take, CancellationToken cancellationToken = default)
        => await _context.VehicleMileageReadings
            .AsNoTracking()
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Una sola consulta para todos los vehículos, y el agrupado se resuelve en memoria.
    ///
    /// Se podría empujar el GROUP BY a la base con Min/Max, pero eso daría por sentado que el
    /// odómetro nunca retrocede — cierto hoy (lo impide ReportVehicleMileage), pero si algún
    /// dato saliera de orden, Min/Max lo taparía en silencio y devolvería un tramo más largo
    /// que el real. Trayendo los extremos de verdad, el calculador puede detectar el dato roto
    /// y no estimar.
    ///
    /// El costo es traer las lecturas de esos vehículos. A escala de un taller es despreciable
    /// (una lectura cada dos semanas por vehículo). Si una flota grande con años de historia lo
    /// hiciera pesar, este es el lugar donde conviene bajar el agrupado a SQL.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, VehicleMileageSpan>> GetSpansByVehiclesAsync(
        IReadOnlyCollection<Guid> vehicleIds, CancellationToken cancellationToken = default)
    {
        if (vehicleIds.Count == 0)
            return new Dictionary<Guid, VehicleMileageSpan>();

        var rows = await _context.VehicleMileageReadings
            .AsNoTracking()
            .Where(r => vehicleIds.Contains(r.VehicleId))
            .Select(r => new { r.VehicleId, r.Mileage, r.CreatedAt })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.VehicleId)
            .Select(g =>
            {
                var ordered = g.OrderBy(r => r.CreatedAt).ToList();
                var first   = ordered[0];
                var last    = ordered[^1];

                return new VehicleMileageSpan(
                    g.Key,
                    first.Mileage, first.CreatedAt,
                    last.Mileage,  last.CreatedAt,
                    ordered.Count);
            })
            .ToDictionary(s => s.VehicleId);
    }
}
