using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

/// <summary>
/// Acceso a la configuración global del taller (singleton). El seeder garantiza
/// que siempre exista una fila al arrancar la app.
/// </summary>
public interface IWorkshopSettingsRepository
{
    /// <summary>Devuelve la fila de settings. Lanza si no existe (debería ser imposible).</summary>
    Task<WorkshopSettings> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Marca la entidad como modificada en el contexto. Hay que llamar SaveChanges aparte.</summary>
    void Update(WorkshopSettings settings);
}
