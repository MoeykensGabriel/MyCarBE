using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Repositories;

public class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(AppDbContext context) : base(context) { }

    public async Task<WorkOrder?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            // Son 8+ Includes de colecciones: en single query Postgres devuelve el
            // producto cartesiano de todas (servicios × fotos × reportes × ...).
            // AsSplitQuery lo parte en una query chica por colección.
            .AsSplitQuery()
            .Include(w => w.Vehicle)
            .Include(w => w.CustomerAtEntry)
            .Include(w => w.FleetAtEntry)
            .Include(w => w.Services.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Photos.Where(p => !p.IsDeleted))
            .Include(w => w.Services.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.AssignedMechanic)
            .Include(w => w.Services.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Area)
            // Reports de inspección con área + mecánico — para que el cliente vea quién
            // revisó cada parte del auto y emparejemos las fotos antes/después por área.
            .Include(w => w.InspectionReports.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Area)
            .Include(w => w.InspectionReports.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Mechanic)
            // CatalogService ya no se incluye: la duración ahora vive en
            // EstimatedDurationMinutesSnapshot del WorkOrderService (soporta ad-hoc).
            .Include(w => w.Parts.Where(p => !p.IsDeleted))
            // Cargamos TODAS las fotos vivas — el FE filtra por target (workOrderServiceId,
            // inspectionReportId, o ninguno = foto general del vehículo). La validación de
            // pasaje a Delivered necesita ver el set completo.
            .Include(w => w.Photos.Where(p => !p.IsDeleted))
            .Include(w => w.StatusChanges.OrderBy(sc => sc.ChangedAt))
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<WorkOrder?> GetWithServicesAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .Include(w => w.Services.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public Task<PagedResult<WorkOrder>> GetAllPagedAsync(WorkOrderStatus? status, IReadOnlyList<WorkOrderStatus>? statuses, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        => PagedAsync(_context.WorkOrders, status, statuses, search, ownerType, page, pageSize, from, to, cancellationToken);

    public Task<PagedResult<WorkOrder>> GetByVehicleIdPagedAsync(Guid vehicleId, WorkOrderStatus? status, IReadOnlyList<WorkOrderStatus>? statuses, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        => PagedAsync(_context.WorkOrders.Where(w => w.VehicleId == vehicleId), status, statuses, search, ownerType, page, pageSize, from, to, cancellationToken);

    public Task<PagedResult<WorkOrder>> GetByCustomerIdAtEntryPagedAsync(Guid customerId, WorkOrderStatus? status, IReadOnlyList<WorkOrderStatus>? statuses, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        => PagedAsync(_context.WorkOrders.Where(w => w.CustomerIdAtEntry == customerId), status, statuses, search, ownerType, page, pageSize, from, to, cancellationToken);

    public Task<PagedResult<WorkOrder>> GetByFleetIdAtEntryPagedAsync(Guid fleetId, WorkOrderStatus? status, IReadOnlyList<WorkOrderStatus>? statuses, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        => PagedAsync(_context.WorkOrders.Where(w => w.FleetIdAtEntry == fleetId), status, statuses, search, ownerType, page, pageSize, from, to, cancellationToken);

    public async Task<WorkOrderService?> GetServiceByIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
        => await _context.WorkOrderServices
            .Include(s => s.WorkOrder)
            .Include(s => s.AssignedMechanic)
            .FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);

    public async Task<IReadOnlyList<WorkOrderService>> GetServicesByMechanicAsync(
        Guid mechanicId,
        WorkOrderServiceAssignmentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkOrderServices
            .AsNoTracking()
            .Include(s => s.WorkOrder)
                .ThenInclude(w => w.Vehicle)
            .Where(s => s.AssignedMechanicId == mechanicId);

        if (status.HasValue)
            query = query.Where(s => s.AssignmentStatus == status.Value);
        else
            // Por defecto: solo activos (Pending o Accepted)
            query = query.Where(s =>
                s.AssignmentStatus == WorkOrderServiceAssignmentStatus.Pending ||
                s.AssignmentStatus == WorkOrderServiceAssignmentStatus.Accepted);

        return await query
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkOrderService>> GetAvailableServicesAsync(CancellationToken cancellationToken = default)
        => await _context.WorkOrderServices
            .AsNoTracking()
            .Include(s => s.WorkOrder)
                .ThenInclude(w => w.Vehicle)
            .Include(s => s.WorkOrder)
                .ThenInclude(w => w.CustomerAtEntry)
            .Include(s => s.WorkOrder)
                .ThenInclude(w => w.FleetAtEntry)
            .Where(s =>
                !s.IsDeleted &&
                s.AssignmentStatus    == WorkOrderServiceAssignmentStatus.Unassigned &&
                s.AssignedMechanicId  == null &&
                s.ApprovalStatus      == Domain.Enums.QuoteItemApprovalStatus.Approved &&
                s.WorkOrder.CurrentStatus == WorkOrderStatus.InProgress)
            // FIFO: los servicios más viejos del pool se ofrecen primero (justo entre mecánicos).
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> GetUnderInspectionWithReportsAsync(CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.Vehicle)
            .Include(w => w.CustomerAtEntry)
            .Include(w => w.FleetAtEntry)
            .Include(w => w.InspectionReports)
            .Where(w => w.CurrentStatus == WorkOrderStatus.UnderInspection)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> GetLateInspectionWindowWithReportsAsync(CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.Vehicle)
            .Include(w => w.InspectionReports)
            // Espeja WorkOrder.AcceptsLateInspection. Son las órdenes que ya cerraron su
            // inspección inicial pero siguen vivas: el auto puede estar en el taller y un
            // área que quedó postergada todavía se puede mirar.
            .Where(w => w.CurrentStatus == WorkOrderStatus.Diagnosing
                     || w.CurrentStatus == WorkOrderStatus.Approved
                     || w.CurrentStatus == WorkOrderStatus.InProgress)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkOrderService>> GetScheduledServicesAsync(
        DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => await _context.WorkOrderServices
            .AsNoTracking()
            .Include(s => s.WorkOrder)
                .ThenInclude(w => w.Vehicle)
            .Include(s => s.AssignedMechanic)
            .Include(s => s.Area)
            .Where(s =>
                !s.IsDeleted &&
                s.ScheduledStart != null &&
                s.ScheduledEnd   != null &&
                s.ScheduledStart <= to &&
                s.ScheduledEnd   >= from)
            .OrderBy(s => s.ScheduledStart)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> GetScheduledWorkOrdersAsync(
        DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.Vehicle)
            .Include(w => w.CustomerAtEntry)
            .Include(w => w.FleetAtEntry)
            // Servicios (con su área) para agrupar el tablero por servicio/área.
            .Include(w => w.Services.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Area)
            .Where(w =>
                w.ScheduledStart != null &&
                w.ScheduledEnd   != null &&
                w.ScheduledStart <= to &&
                w.ScheduledEnd   >= from &&
                (w.CurrentStatus == WorkOrderStatus.Approved ||
                 w.CurrentStatus == WorkOrderStatus.InProgress ||
                 w.CurrentStatus == WorkOrderStatus.Completed))
            .OrderBy(w => w.ScheduledStart)
            .ToListAsync(cancellationToken);

    private static async Task<PagedResult<WorkOrder>> PagedAsync(
        IQueryable<WorkOrder> query, WorkOrderStatus? status, IReadOnlyList<WorkOrderStatus>? statuses, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        query = query
            .AsNoTracking()
            .Include(w => w.Vehicle)
            .Include(w => w.CustomerAtEntry)
            .Include(w => w.FleetAtEntry)
            // Include FILTRADO a propósito: alimenta WorkOrder.HasLateFindings para avisarle
            // a la oficina que llegó un hallazgo con la inspección ya cerrada. Al filtrar acá
            // normalmente no trae ninguna fila, así que no encarece el listado.
            .Include(w => w.InspectionReports.Where(r => r.IsLate && r.HasIssue));

        if (statuses is { Count: > 0 })
            query = query.Where(w => statuses.Contains(w.CurrentStatus));
        else if (status.HasValue)
            query = query.Where(w => w.CurrentStatus == status.Value);

        if (ownerType.HasValue)
            query = ownerType.Value == WorkOrderOwnerType.Fleet
                ? query.Where(w => w.FleetIdAtEntry != null)
                : query.Where(w => w.CustomerIdAtEntry != null);

        // Filtro de rango de fecha de creación (inclusive)
        if (from.HasValue)
            query = query.Where(w => w.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(w => w.CreatedAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            // Número de orden: es como la identifica el taller por teléfono, así que buscarlo
            // tiene que dar exacto y no mezclado con patentes que contengan esos dígitos.
            // Aceptamos el "#" que el usuario copia de la pantalla.
            var numberTerm = term.TrimStart('#');
            int? number = int.TryParse(numberTerm, out var parsed) ? parsed : null;

            query = query.Where(w =>
                (number != null && w.Number == number) ||
                w.Vehicle.LicensePlate.ToLower().Contains(term) ||
                (w.CustomerAtEntry != null &&
                    (w.CustomerAtEntry.FirstName + " " + w.CustomerAtEntry.LastName).ToLower().Contains(term)) ||
                (w.FleetAtEntry != null &&
                    w.FleetAtEntry.CompanyName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkOrder>(items, totalCount, page, pageSize);
    }
}
