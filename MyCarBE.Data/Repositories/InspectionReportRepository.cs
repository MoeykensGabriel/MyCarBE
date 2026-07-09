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

    public async Task<IReadOnlyList<InspectionReport>> GetSkippedForVehicleLastOrderAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        // Última orden no cancelada del vehículo — si no tiene omitidas, no hay aviso.
        var lastOrderId = await _context.WorkOrders
            .Where(w => w.VehicleId == vehicleId && w.CurrentStatus != Domain.Enums.WorkOrderStatus.Cancelled)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastOrderId is null)
            return Array.Empty<InspectionReport>();

        return await _context.InspectionReports
            .Include(r => r.Area)
            .Include(r => r.WorkOrder)
            .Where(r => r.WorkOrderId == lastOrderId && r.IsSkipped)
            .OrderBy(r => r.Area.Name)
            .ToListAsync(cancellationToken);
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
