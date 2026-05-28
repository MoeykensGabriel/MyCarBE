using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IAreaRepository : IRepository<Area>
{
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid excludeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Area>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve áreas por Ids (para validar bulk de AreaIds al asignar a un mecánico).
    /// </summary>
    Task<IReadOnlyList<Area>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
