using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<bool> DocumentNumberExistsAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default);
}
