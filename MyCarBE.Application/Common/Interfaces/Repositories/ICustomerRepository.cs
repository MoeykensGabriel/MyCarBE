using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);

    /// <summary>Used on Create — no exclusion needed.</summary>
    Task<bool> DocumentNumberExistsAsync(string documentNumber, CancellationToken cancellationToken = default);

    /// <summary>Used on Update — excludes the customer being updated.</summary>
    Task<bool> DocumentNumberExistsAsync(string documentNumber, Guid excludeId, CancellationToken cancellationToken = default);

    /// <summary>Used on Create — no exclusion needed.</summary>
    Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default);

    /// <summary>Used on Update — excludes the customer being updated.</summary>
    Task<bool> PhoneExistsAsync(string phone, Guid excludeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Customer>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default);
}
