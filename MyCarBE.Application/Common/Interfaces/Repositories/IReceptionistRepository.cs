using MyCarBE.Application.Common.Models;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IReceptionistRepository : IRepository<Receptionist>
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid excludeId, CancellationToken cancellationToken = default);

    Task<Receptionist?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);

    Task<PagedResult<Receptionist>> SearchPagedAsync(
        string? searchTerm,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
