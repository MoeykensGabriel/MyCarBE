using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class InspectionReportRepository : Repository<InspectionReport>, IInspectionReportRepository
{
    public InspectionReportRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<InspectionReport>> GetByWorkOrderAsync(Guid workOrderId, CancellationToken cancellationToken = default)
        => await _context.InspectionReports
            .Include(r => r.Area)
            .Include(r => r.Mechanic)
            .Include(r => r.Photos)
            .Include(r => r.ProposedServices)
            .Include(r => r.ProposedParts)
            .Where(r => r.WorkOrderId == workOrderId)
            .OrderBy(r => r.Area.Name)
            .ToListAsync(cancellationToken);

    public async Task<InspectionReport?> GetByWorkOrderAndAreaAsync(Guid workOrderId, Guid areaId, CancellationToken cancellationToken = default)
        => await _context.InspectionReports
            .Include(r => r.Area)
            .Include(r => r.Mechanic)
            .Include(r => r.Photos)
            .Include(r => r.ProposedServices)
            .Include(r => r.ProposedParts)
            .FirstOrDefaultAsync(r => r.WorkOrderId == workOrderId && r.AreaId == areaId, cancellationToken);

    public async Task<bool> ExistsForAreaAsync(Guid workOrderId, Guid areaId, CancellationToken cancellationToken = default)
        => await _context.InspectionReports
            .AnyAsync(r => r.WorkOrderId == workOrderId && r.AreaId == areaId, cancellationToken);

    public async Task<InspectionReport?> GetByIdWithProposalsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.InspectionReports
            .Include(r => r.ProposedServices)
            .Include(r => r.ProposedParts)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<InspectionReport>> GetByWorkOrderWithProposalsAsync(Guid workOrderId, CancellationToken cancellationToken = default)
        => await _context.InspectionReports
            .Include(r => r.ProposedServices)
            .Include(r => r.ProposedParts)
            .Where(r => r.WorkOrderId == workOrderId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<InspectionReport>> GetPendingSkippedForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        // Historial completo de inspecciones del vehículo, no solo el de la última orden.
        // Mirar solo la última orden hacía desaparecer el aviso apenas se abría la orden
        // nueva (esa orden todavía no tiene reportes), justo cuando el taller lo necesitaba.
        // El query filter global ya descarta los reportes deshechos (soft delete).
        var reports = await _context.InspectionReports
            .Include(r => r.Area)
            .Include(r => r.WorkOrder)
            .Where(r => r.WorkOrder.VehicleId == vehicleId
                     && r.WorkOrder.CurrentStatus != Domain.Enums.WorkOrderStatus.Cancelled)
            .ToListAsync(cancellationToken);

        // Manda el reporte más reciente de cada área: si es una postergación, el área sigue
        // debiendo; si alguien la inspectó (o la cerró "sin novedades") después, ya no.
        // El desempate por Id mantiene el resultado estable entre órdenes del mismo instante.
        return reports
            .GroupBy(r => r.AreaId)
            .Select(g => g
                .OrderByDescending(r => r.WorkOrder.CreatedAt)
                .ThenByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .First())
            .Where(r => r.IsSkipped)
            .OrderBy(r => r.Area.Name)
            .ToList();
    }

    public async Task<bool> IsAreaPendingForVehicleAsync(Guid vehicleId, Guid areaId, CancellationToken cancellationToken = default)
    {
        var pending = await GetPendingSkippedForVehicleAsync(vehicleId, cancellationToken);
        return pending.Any(r => r.AreaId == areaId);
    }

    public void RemoveAllProposals(InspectionReport report)
    {
        if (report.ProposedServices.Count > 0)
            _context.InspectionReportProposedServices.RemoveRange(report.ProposedServices);

        if (report.ProposedParts.Count > 0)
            _context.InspectionReportProposedParts.RemoveRange(report.ProposedParts);

        // El interceptor en AppDbContext.SaveChangesAsync convierte Remove en soft delete.
    }
}
