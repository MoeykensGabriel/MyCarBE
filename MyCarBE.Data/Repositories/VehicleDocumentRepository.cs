using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class VehicleDocumentRepository : Repository<VehicleDocument>, IVehicleDocumentRepository
{
    public VehicleDocumentRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<VehicleDocument>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => await _context.VehicleDocuments
            .AsNoTracking()
            .Where(d => d.VehicleId == vehicleId)
            .OrderBy(d => d.ExpiresOn)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UpcomingExpirationDto>> GetUpcomingForCustomerAsync(
        Guid customerId, int horizonDays, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var limit = today.AddDays(horizonDays);

        return await _context.VehicleDocuments
            .Where(d => d.Vehicle.CustomerId == customerId && d.ExpiresOn <= limit)
            .OrderBy(d => d.ExpiresOn)
            .Select(d => new UpcomingExpirationDto(
                d.Id,
                d.VehicleId,
                d.Vehicle.LicensePlate,
                d.Vehicle.Brand,
                d.Vehicle.Model,
                d.DocumentType,
                d.ExpiresOn,
                d.ExpiresOn.DayNumber - today.DayNumber
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UpcomingExpirationDto>> GetUpcomingForFleetAsync(
        Guid fleetId, int horizonDays, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var limit = today.AddDays(horizonDays);

        return await _context.VehicleDocuments
            .Where(d => d.Vehicle.FleetId == fleetId && d.ExpiresOn <= limit)
            .OrderBy(d => d.ExpiresOn)
            .Select(d => new UpcomingExpirationDto(
                d.Id,
                d.VehicleId,
                d.Vehicle.LicensePlate,
                d.Vehicle.Brand,
                d.Vehicle.Model,
                d.DocumentType,
                d.ExpiresOn,
                d.ExpiresOn.DayNumber - today.DayNumber
            ))
            .ToListAsync(cancellationToken);
    }
}
