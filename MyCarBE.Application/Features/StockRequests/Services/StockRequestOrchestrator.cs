using Microsoft.Extensions.Logging;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.StockRequests.Services;

public class StockRequestOrchestrator : IStockRequestOrchestrator
{
    private readonly IPartsStockRequestRepository _repository;
    private readonly IStockService                _stockService;
    private readonly ILogger<StockRequestOrchestrator> _logger;

    public StockRequestOrchestrator(
        IPartsStockRequestRepository       repository,
        IStockService                      stockService,
        ILogger<StockRequestOrchestrator>  logger)
    {
        _repository    = repository;
        _stockService  = stockService;
        _logger        = logger;
    }

    public async Task EnsureStockRequestForApprovedWorkOrderAsync(
        WorkOrder workOrder,
        CancellationToken cancellationToken = default)
    {
        // Idempotencia: si ya se creó un pedido para esta WO, no hacer nada.
        // Esto puede pasar si la WO transiciona varias veces (ej. el admin la pasa a
        // Approved y después a InProgress — ambos disparan el orchestrator).
        var existing = await _repository.GetByWorkOrderIdAsync(workOrder.Id, cancellationToken);
        if (existing is not null)
        {
            _logger.LogDebug(
                "Stock request ya existe para WorkOrder {WorkOrderId} (Id={RequestId}). Skip.",
                workOrder.Id, existing.Id);
            return;
        }

        // Tomar todos los repuestos aprobados con ProductCode. Los custom (ProductCode = null)
        // no pasan por el depósito — el taller los compra suelto.
        var partsToOrder = workOrder.Parts
            .Where(p => !p.IsDeleted
                     && p.ApprovalStatus == QuoteItemApprovalStatus.Approved
                     && !string.IsNullOrWhiteSpace(p.ProductCode))
            .ToList();

        if (partsToOrder.Count == 0)
        {
            _logger.LogInformation(
                "WorkOrder {WorkOrderId} aprobada sin repuestos del catálogo — no se genera pedido al depósito.",
                workOrder.Id);
            return;
        }

        // Resolver patente: si por algún motivo no hay vehículo cargado, fallback a "—"
        // en lugar de romper el flujo de aprobación. El depósito puede pedir clarificación
        // por canal manual.
        var licensePlate = workOrder.Vehicle?.LicensePlate?.Trim() ?? "—";

        var stockRequest = new PartsStockRequest
        {
            Id                    = Guid.NewGuid(),
            WorkOrderId           = workOrder.Id,
            LicensePlateSnapshot  = licensePlate,
            // Condición de venta al momento de aprobar — el depósito la usa para
            // decidir si compra (CC directo; OC con número; Contado con la seña).
            SaleConditionSnapshot = workOrder.SaleCondition,
            OcNumberSnapshot      = workOrder.PurchaseOrderNumber,
            DepositAmountSnapshot = workOrder.DepositAmount,
            Status                = StockRequestStatus.PendingReview,
            Items                = partsToOrder.Select(p => new PartsStockRequestItem
            {
                Id              = Guid.NewGuid(),
                WorkOrderPartId = p.Id,
                ProductCode     = p.ProductCode!,
                Name            = p.Name,
                Quantity        = p.Quantity,
                Status          = StockRequestItemStatus.PendingReview,
            }).ToList()
        };

        await _repository.AddAsync(stockRequest, cancellationToken);

        // Enviar a GestionPGB (stub por ahora). Si falla, no rompemos la aprobación —
        // queda el pedido persistido en PendingReview y se puede reintentar manualmente.
        try
        {
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
            }
            else
            {
                _logger.LogWarning(
                    "GestionPGB rechazó el pedido para WorkOrder {WorkOrderId}: {Error}",
                    workOrder.Id, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fallo enviando pedido a GestionPGB para WorkOrder {WorkOrderId}. El pedido queda persistido en PendingReview para reintento manual.",
                workOrder.Id);
            // Se hace swallow intencional: la aprobación de la WO no debe fallar por el depósito.
        }
    }
}
