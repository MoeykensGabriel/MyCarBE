using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default)
        => await _context.Customers
            .FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber, cancellationToken);

    public async Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
        => await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == phone, cancellationToken);

    public async Task<bool> DocumentNumberExistsAsync(string documentNumber, CancellationToken cancellationToken = default)
        => await _context.Customers
            .AnyAsync(c => c.DocumentNumber == documentNumber, cancellationToken);

    public async Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default)
        => await _context.Customers
            .AnyAsync(c => c.Phone == phone, cancellationToken);
}
