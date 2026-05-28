using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IPartsStockRequestRepository : IRepository<PartsStockRequest>
{
    /// <summary>Devuelve el pedido (con items) de una WO, o null si todavía no se creó.</summary>
    Task<PartsStockRequest?> GetByWorkOrderIdAsync(Guid workOrderId, CancellationToken cancellationToken = default);

    /// <summary>Devuelve el pedido con items + WorkOrder y Vehicle para la pantalla de seguimiento.</summary>
    Task<PartsStockRequest?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Listado de pedidos activos para la oficina. Filtros opcionales por estado y patente.
    /// </summary>
    Task<IReadOnlyList<PartsStockRequest>> GetAllFilteredAsync(
        StockRequestStatus? status,
        string? licensePlate,
        CancellationToken cancellationToken = default);

    /// <summary>Devuelve un item individual con su pedido padre, o null.</summary>
    Task<PartsStockRequestItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken = default);
}
