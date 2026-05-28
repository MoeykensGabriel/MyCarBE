using MyCarBE.Application.Common.Models;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IMechanicRepository : IRepository<Mechanic>
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid excludeId, CancellationToken cancellationToken = default);

    /// <summary>Resuelve el Mechanic vinculado al ApplicationUser del JWT (sub claim).</summary>
    Task<Mechanic?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);

    /// <summary>Mechanic con sus Areas eager-loaded (para edición de asignación y listados).</summary>
    Task<Mechanic?> GetByIdWithAreasAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Mechanic>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<Mechanic>> SearchPagedAsync(
        string? searchTerm,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
