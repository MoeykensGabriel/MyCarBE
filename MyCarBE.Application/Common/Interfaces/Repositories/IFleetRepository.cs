using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IFleetRepository : IRepository<Fleet>
{
    Task<Fleet?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);
    Task<bool> TaxIdExistsAsync(string taxId, CancellationToken cancellationToken = default);
}
