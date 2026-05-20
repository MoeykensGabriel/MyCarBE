using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface ICatalogServiceRepository : IRepository<CatalogService>
{
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogService>> GetAllWithInactiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogService>> GetActiveAsync(CancellationToken cancellationToken = default);
}
