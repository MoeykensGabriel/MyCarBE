using MediatR;
using Microsoft.Extensions.Logging;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.StockRequests.Commands.HandleStockCallback;

public class HandleStockCallbackCommandHandler
    : IRequestHandler<HandleStockCallbackCommand, Unit>
{
    private readonly IPartsStockRequestRepository _repository;
    private readonly IUnitOfWork                  _unitOfWork;
    private readonly ILogger<HandleStockCallbackCommandHandler> _logger;

    public HandleStockCallbackCommandHandler(
        IPartsStockRequestRepository repository,
        IUnitOfWork                  unitOfWork,
        ILogger<HandleStockCallbackCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger     = logger;
    }

    public async Task<Unit> Handle(HandleStockCallbackCommand request, CancellationToken cancellationToken)
    {
        // GestionPGB nos pasa SU id de pedido como WorkshopOrderId. Nosotros lo guardamos
        // en ExternalReference al crear el pedido.
        var stockRequest = await FindByExternalReferenceAsync(request.WorkshopOrderId, cancellationToken);
        if (stockRequest is null)
        {
            _logger.LogWarning(
                "Callback recibido para WorkshopOrderId {WorkshopOrderId} (patente {Plate}) pero no existe el pedido localmente.",
                request.WorkshopOrderId, request.LicensePlate);
            throw new NotFoundException(nameof(PartsStockRequest), request.WorkshopOrderId);
        }

        _logger.LogInformation(
            "Callback de GestionPGB recibido para pedido {RequestId} (patente {Plate}): status={Status}, items={Count}",
            stockRequest.Id, request.LicensePlate, request.Status, request.Items.Count);

        // Actualizar cada item local matcheando por ProductCode.
        foreach (var theirItem in request.Items)
        {
            var ourItem = stockRequest.Items
                .FirstOrDefault(i => !i.IsDeleted &&
                                     string.Equals(i.ProductCode, theirItem.ProductCode, StringComparison.Ordinal));

            if (ourItem is null)
            {
                _logger.LogWarning(
                    "Callback incluye ProductCode {Code} que no está en el pedido local {RequestId}. Skip.",
                    theirItem.ProductCode, stockRequest.Id);
                continue;
            }

            var newStatus = MapItemStatus(theirItem.Status);
            ourItem.UpdateStatus(newStatus, notes: null);
        }

        stockRequest.RecomputeStatus();
        _repository.Update(stockRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task<PartsStockRequest?> FindByExternalReferenceAsync(
        Guid workshopOrderId, CancellationToken cancellationToken)
    {
        // Buscamos en toda la lista — la cantidad de pedidos activos es chica.
        var all = await _repository.GetAllFilteredAsync(status: null, licensePlate: null, cancellationToken);
        return all.FirstOrDefault(r => r.ExternalReference == workshopOrderId.ToString());
    }

    /// <summary>Mapea el string del enum WorkshopItemStatus de GestionPGB a nuestro enum.</summary>
    private static StockRequestItemStatus MapItemStatus(string status) => status switch
    {
        "Available"          => StockRequestItemStatus.Available,
        "Shortage"           => StockRequestItemStatus.Missing,
        "NotFound"           => StockRequestItemStatus.Missing,
        "PurchasedInTransit" => StockRequestItemStatus.InTransit,
        "Delivered"          => StockRequestItemStatus.Delivered,
        _                    => StockRequestItemStatus.PendingReview,
    };
}
