using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
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

    public async Task<PagedResult<Customer>> SearchPagedAsync(
        string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
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

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Customer>(items, totalCount, page, pageSize);
    }

    public async Task<Customer?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
        => await _context.Customers
            .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId, cancellationToken);

    public async Task<bool> FleetContactExistsAsync(Guid fleetId, CancellationToken cancellationToken = default)
        => await _context.Customers
            .AnyAsync(c => c.FleetId == fleetId, cancellationToken);

    public async Task<Customer?> GetByFleetIdAsync(Guid fleetId, CancellationToken cancellationToken = default)
        => await _context.Customers
            .FirstOrDefaultAsync(c => c.FleetId == fleetId, cancellationToken);
}
