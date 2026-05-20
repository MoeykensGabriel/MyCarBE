using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IFleetRepository : IRepository<Fleet>
{
    Task<Fleet?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);

    /// <summary>Returns fleet including Contacts and Vehicles collections.</summary>
    Task<Fleet?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Used on Create — no exclusion needed.</summary>
    Task<bool> TaxIdExistsAsync(string taxId, CancellationToken cancellationToken = default);

    /// <summary>Used on Update — excludes the fleet being updated.</summary>
    Task<bool> TaxIdExistsAsync(string taxId, Guid excludeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Fleet>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default);
}
