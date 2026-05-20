using MyCarBE.Application.Common.Models;
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

    Task<PagedResult<Customer>> SearchPagedAsync(string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Resolves the Customer linked to the given ApplicationUser Id (from JWT sub claim).</summary>
    Task<Customer?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);

    /// <summary>Verifica si una flota ya tiene un contacto asignado.</summary>
    Task<bool> FleetContactExistsAsync(Guid fleetId, CancellationToken cancellationToken = default);

    /// <summary>Devuelve el contacto encargado de la flota.</summary>
    Task<Customer?> GetByFleetIdAsync(Guid fleetId, CancellationToken cancellationToken = default);
}
