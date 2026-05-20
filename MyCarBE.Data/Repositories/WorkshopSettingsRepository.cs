using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class WorkshopSettingsRepository : IWorkshopSettingsRepository
{
    private readonly AppDbContext _context;

    public WorkshopSettingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkshopSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        // Convención: siempre debe existir exactamente una fila (la garantiza el seeder).
        // FirstAsync tira si no hay nada — eso indicaría un bug operacional, no de usuario.
        return await _context.WorkshopSettings.FirstAsync(cancellationToken);
    }

    public void Update(WorkshopSettings settings) => _context.WorkshopSettings.Update(settings);
}
