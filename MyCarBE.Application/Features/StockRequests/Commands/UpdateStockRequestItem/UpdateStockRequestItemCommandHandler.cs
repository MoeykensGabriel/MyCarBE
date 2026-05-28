using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.StockRequests.DTOs;
using MyCarBE.Application.Features.StockRequests.Queries.GetStockRequests;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.StockRequests.Commands.UpdateStockRequestItem;

public class UpdateStockRequestItemCommandHandler
    : IRequestHandler<UpdateStockRequestItemCommand, StockRequestDto>
{
    private readonly IPartsStockRequestRepository _repository;
    private readonly IUnitOfWork                  _unitOfWork;

    public UpdateStockRequestItemCommandHandler(
        IPartsStockRequestRepository repository,
        IUnitOfWork                  unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StockRequestDto> Handle(
        UpdateStockRequestItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await _repository.GetItemByIdAsync(request.ItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(PartsStockRequestItem), request.ItemId);

        item.UpdateStatus(request.NewStatus, request.Notes);

        // Recalcular el estado agregado del pedido a partir de los items actualizados.
        item.PartsStockRequest.RecomputeStatus();

        _repository.Update(item.PartsStockRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Devolvemos el pedido completo para que el webhook caller pueda confirmar el efecto.
        var refreshed = await _repository.GetWithDetailsAsync(item.PartsStockRequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(PartsStockRequest), item.PartsStockRequestId);

        return GetStockRequestsQueryHandler.ToDto(refreshed);
    }
}
