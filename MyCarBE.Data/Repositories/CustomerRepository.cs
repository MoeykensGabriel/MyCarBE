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

    public async Task<bool> DocumentNumberExistsAsync(string documentNumber, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Customers
            .AnyAsync(c => c.DocumentNumber == documentNumber && c.Id != excludeId, cancellationToken);

    public async Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default)
        => await _context.Customers
            .AnyAsync(c => c.Phone == phone, cancellationToken);

    public async Task<bool> PhoneExistsAsync(string phone, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Customers
            .AnyAsync(c => c.Phone == phone && c.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<Customer>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term)  ||
                c.DocumentNumber.ToLower().Contains(term) ||
                c.Phone.Contains(term));
        }

        return await query
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }
}
