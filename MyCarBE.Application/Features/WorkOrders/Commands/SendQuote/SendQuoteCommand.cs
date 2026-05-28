using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.SendQuote;

/// <summary>
/// Envía el presupuesto al cliente y transiciona la orden de Diagnosing → AwaitingApproval.
///
/// Atómicamente (en una sola transacción):
///   1) Congela todos los items activos (services + parts) — quedan inmutables.
///   2) Genera token de aprobación de 64 chars.
///   3) Setea WorkOrder.QuoteExpiresAt = now + 30 días.
///   4) Transiciona el estado a AwaitingApproval (registra el evento en el timeline).
///
/// Post-commit (fire-and-forget):
///   5) Genera el PDF y envía email al cliente con el link de aprobación.
///
/// Este es el ÚNICO camino válido a AwaitingApproval. ChangeStatus rechaza esa transición
/// directamente para garantizar que items siempre queden frozen + QuoteExpiresAt seteado.
/// </summary>
public record SendQuoteCommand(Guid WorkOrderId) : IRequest<WorkOrderDetailDto>;
