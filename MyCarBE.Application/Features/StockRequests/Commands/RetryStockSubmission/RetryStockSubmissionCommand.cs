using MediatR;
using MyCarBE.Application.Features.StockRequests.DTOs;

namespace MyCarBE.Application.Features.StockRequests.Commands.RetryStockSubmission;

/// <summary>
/// Reintenta el envío de un pedido a GestionPGB cuando la llamada original falló
/// (p.ej. el depósito no estaba corriendo en el momento de la aprobación).
/// Solo es válido para pedidos cuyo ExternalReference todavía es null.
/// </summary>
public record RetryStockSubmissionCommand(Guid StockRequestId)
    : IRequest<StockRequestDto>;
