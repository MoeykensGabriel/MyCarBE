using MediatR;
using Microsoft.Extensions.Logging;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.StockRequests.DTOs;
using MyCarBE.Application.Features.StockRequests.Queries.GetStockRequests;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.StockRequests.Commands.RetryStockSubmission;

public class RetryStockSubmissionCommandHandler
    : IRequestHandler<RetryStockSubmissionCommand, StockRequestDto>
{
    private readonly IPartsStockRequestRepository _repository;
    private readonly IStockService                _stockService;
    private readonly IUnitOfWork                  _unitOfWork;
    private readonly ILogger<RetryStockSubmissionCommandHandler> _logger;

    public RetryStockSubmissionCommandHandler(
        IPartsStockRequestRepository                 repository,
        IStockService                                stockService,
        IUnitOfWork                                  unitOfWork,
        ILogger<RetryStockSubmissionCommandHandler>  logger)
    {
        _repository   = repository;
        _stockService = stockService;
        _unitOfWork   = unitOfWork;
        _logger       = logger;
    }

    public async Task<StockRequestDto> Handle(
        RetryStockSubmissionCommand request,
        CancellationToken cancellationToken)
    {
        var stockRequest = await _repository.GetWithDetailsAsync(request.StockRequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(PartsStockRequest), request.StockRequestId);

        // Si ya tiene referencia externa el pedido ya fue enviado con éxito.
        if (!string.IsNullOrWhiteSpace(stockRequest.ExternalReference))
        {
            _logger.LogWarning(
                "RetryStockSubmission ignorado: StockRequest {Id} ya tiene ExternalReference={Ref}.",
                stockRequest.Id, stockRequest.ExternalReference);
            return GetStockRequestsQueryHandler.ToDto(stockRequest);
        }

        _logger.LogInformation(
            "Reintentando envío a GestionPGB para StockRequest {Id} (WO {WoId}).",
            stockRequest.Id, stockRequest.WorkOrderId);

        var result = await _stockService.SubmitRequestAsync(stockRequest, cancellationToken);

        if (result.Success)
        {
            stockRequest.ExternalReference = result.ExternalReference;

            foreach (var initial in result.ItemStatuses)
            {
                var item = stockRequest.Items.FirstOrDefault(i => i.Id == initial.ItemId);
                if (item is not null)
                    item.UpdateStatus(initial.Status, initial.Notes);
            }

            stockRequest.RecomputeStatus();
            _repository.Update(stockRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Reintento exitoso para StockRequest {Id}. ExternalReference={Ref}.",
                stockRequest.Id, stockRequest.ExternalReference);
        }
        else
        {
            _logger.LogWarning(
                "GestionPGB volvió a rechazar el reintento para StockRequest {Id}: {Error}",
                stockRequest.Id, result.Error);
        }

        var refreshed = await _repository.GetWithDetailsAsync(stockRequest.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PartsStockRequest), stockRequest.Id);

        return GetStockRequestsQueryHandler.ToDto(refreshed);
    }
}
