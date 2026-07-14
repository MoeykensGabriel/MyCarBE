using Microsoft.Extensions.Logging;
using MyCarBE.Application.Common.Formatting;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Implementación de IQuoteDeliveryService. Encapsula:
///   1) Resolución de destinatario (Customer directo o contacto de Fleet)
///   2) Generación del PDF con QuestPDF (vía IPdfService)
///   3) Disparo asincrónico del email (fire-and-forget para no bloquear al admin)
///
/// El fire-and-forget captura SOLO primitivas — el DbContext del request puede
/// disponerse mientras el task de SMTP sigue corriendo.
/// </summary>
public sealed class QuoteDeliveryService : IQuoteDeliveryService
{
    private readonly IVehicleRepository  _vehicleRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPdfService         _pdfService;
    private readonly IEmailService       _emailService;
    private readonly ILogger<QuoteDeliveryService> _logger;

    public QuoteDeliveryService(
        IVehicleRepository  vehicleRepository,
        ICustomerRepository customerRepository,
        IPdfService         pdfService,
        IEmailService       emailService,
        ILogger<QuoteDeliveryService> logger)
    {
        _vehicleRepository  = vehicleRepository;
        _customerRepository = customerRepository;
        _pdfService         = pdfService;
        _emailService       = emailService;
        _logger             = logger;
    }

    public async Task SendQuoteEmailAsync(
        WorkOrder workOrder,
        WorkOrderDetailDto dto,
        string approvalLink,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(workOrder.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            _logger.LogWarning("Quote email skipped: vehicle {VehicleId} not found.", workOrder.VehicleId);
            return;
        }

        string recipientName, recipientEmail;

        if (workOrder.CustomerIdAtEntry.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(workOrder.CustomerIdAtEntry.Value, cancellationToken);
            if (customer is null || string.IsNullOrEmpty(customer.Email)) return;
            recipientName  = $"{customer.FirstName} {customer.LastName}";
            recipientEmail = customer.Email;
        }
        else if (workOrder.FleetIdAtEntry.HasValue)
        {
            var contact = await _customerRepository.GetByFleetIdAsync(workOrder.FleetIdAtEntry.Value, cancellationToken);
            if (contact is null || string.IsNullOrEmpty(contact.Email)) return;
            recipientName  = $"{contact.FirstName} {contact.LastName}";
            recipientEmail = contact.Email;
        }
        else
        {
            _logger.LogWarning("Quote email skipped: WO {WorkOrderId} has no Customer nor Fleet.", workOrder.Id);
            return;
        }

        // Snapshot de todo lo que el task fire-and-forget va a necesitar.
        // Nada que dependa de un scoped DbContext puede capturarse acá.
        var brand        = vehicle.Brand;
        var model        = vehicle.Model;
        var year         = vehicle.Year;
        var licensePlate = vehicle.LicensePlate;
        var total        = dto.TotalAmount;
        var name         = recipientName;
        var email        = recipientEmail;

        var pdfBytes = _pdfService.GenerateQuotePdf(new QuotePdfData(
            WorkOrder:      dto,
            LicensePlate:   licensePlate,
            VehicleBrand:   brand,
            VehicleModel:   model,
            VehicleYear:    year,
            RecipientName:  name,
            RecipientEmail: email));

        _ = FireAndForgetSendAsync(email, brand, model, licensePlate, name, total, approvalLink, pdfBytes);
    }

    private async Task FireAndForgetSendAsync(
        string to, string brand, string model, string licensePlate,
        string recipientName, decimal totalAmount, string approvalLink, byte[] pdfBytes)
    {
        try
        {
            await _emailService.SendAsync(
                to:                to,
                subject:           $"Presupuesto para su vehículo {brand} {model} — MyCarApp",
                htmlBody:          BuildBody(recipientName, brand, model, totalAmount, approvalLink),
                attachment:        pdfBytes,
                attachmentName:    $"Presupuesto-{licensePlate}-{DateTime.UtcNow:yyyyMMdd}.pdf",
                cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email de presupuesto a {Email}", to);
        }
    }

    private static string BuildBody(string name, string brand, string model, decimal total, string approvalLink) => $"""
        <h2>Hola, {name}!</h2>
        <p>El diagnóstico de tu <strong>{brand} {model}</strong> está listo.</p>
        <p>Adjuntamos el presupuesto detallado. El total estimado es de <strong>{MoneyFormat.ArCurrency(total)}</strong>.</p>
        <p>Para revisar y aprobar los items, hacé clic en el siguiente botón:</p>
        <p style="margin:24px 0;">
          <a href="{approvalLink}"
             style="background:#1d4ed8;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:bold;display:inline-block;">
            Revisar presupuesto
          </a>
        </p>
        <p style="color:#6b7280;font-size:0.875rem;">
          Este enlace es válido por 30 días. Si no solicitaste este presupuesto, ignorá este mensaje.
        </p>
        <br>
        <p><em>MyCarApp — Taller de Servicios Automotores</em></p>
        """;
}
