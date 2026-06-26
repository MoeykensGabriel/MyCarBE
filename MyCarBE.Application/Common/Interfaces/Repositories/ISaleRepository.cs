using MyCarBE.Application.Common.Models;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface ISaleRepository : IRepository<Sale>
{
    /// <summary>Venta por id con sus ítems + comprador (cliente/flota) cargados.</summary>
    Task<Sale?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ventas paginadas (más nuevas primero) con ítems + comprador, filtrables por
    /// cliente/flota/vendedor y rango de fechas (sobre CreatedAt).
    /// </summary>
    Task<PagedResult<Sale>> GetPagedAsync(
        Guid? customerId,
        Guid? fleetId,
        Guid? sellerUserId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
