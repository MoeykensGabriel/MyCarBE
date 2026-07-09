using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ReviseQuote;

/// <summary>
/// "Modificar presupuesto": el cliente pidió cambios antes de aprobar. Vuelve la orden
/// de AwaitingApproval → Diagnosing para editar los items y reenviar.
///
/// Atómicamente (en una sola transacción):
///   1) Transiciona a Diagnosing (queda registrado en el timeline con la nota).
///   2) Descongela todos los items activos (FrozenAt = null) y los resetea a Pending.
///   3) Limpia QuoteExpiresAt (el TTL cleanup no debe cancelar una orden en edición).
///   4) Invalida el token de aprobación activo — el link del email viejo deja de funcionar.
///
/// Después de editar, el admin reenvía con SendQuote (nuevo token + nuevo email).
/// </summary>
public record ReviseQuoteCommand(Guid WorkOrderId, string? Note = null) : IRequest<WorkOrderDetailDto>;
