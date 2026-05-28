using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Orquesta el envío del presupuesto al cliente: resuelve destinatario, genera PDF,
/// y dispara el envío de email en background (fire-and-forget).
///
/// Toda la lectura de DB ocurre sincrónicamente dentro del scope del request — solo
/// el SMTP call queda en background, capturando primitivas (no entidades scoped).
/// </summary>
public interface IQuoteDeliveryService
{
    /// <summary>
    /// Pre-carga vehiculo + destinatario, genera el PDF y dispara el envío del email.
    /// No throws — los errores de SMTP se logean. Si no se puede resolver destinatario,
    /// no hace nada (situación válida para órdenes legacy sin email).
    /// </summary>
    Task SendQuoteEmailAsync(
        WorkOrder           workOrder,
        WorkOrderDetailDto  dto,
        string              approvalLink,
        CancellationToken   cancellationToken);
}
