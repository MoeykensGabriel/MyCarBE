using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Common;

namespace MyCarBE.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Set<T>().ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _context.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity)
        => _context.Set<T>().Update(entity);

    /// <summary>
    /// Llama a Remove() pero SaveChangesAsync lo intercepta y convierte en soft delete.
    /// Nunca hay un DELETE físico en la DB.
    /// </summary>
    public void Delete(T entity)
        => _context.Set<T>().Remove(entity);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Set<T>().AnyAsync(e => e.Id == id, cancellationToken);
}
