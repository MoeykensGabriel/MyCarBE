using System.Security.Cryptography;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ChangeWorkOrderStatus;

public class ChangeWorkOrderStatusCommandHandler : IRequestHandler<ChangeWorkOrderStatusCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository              _workOrderRepository;
    private readonly IVehicleRepository                _vehicleRepository;
    private readonly ICustomerRepository               _customerRepository;
    private readonly IFleetRepository                  _fleetRepository;
    private readonly IWorkOrderApprovalTokenRepository _tokenRepository;
    private readonly ICurrentUserService               _currentUser;
    private readonly IPdfService                       _pdfService;
    private readonly IEmailService                     _emailService;
    private readonly IUnitOfWork                       _unitOfWork;
    private readonly IMapper                           _mapper;
    private readonly IApprovalLinkBuilder              _approvalLinkBuilder;
    private readonly ILogger<ChangeWorkOrderStatusCommandHandler> _logger;

    public ChangeWorkOrderStatusCommandHandler(
        IWorkOrderRepository              workOrderRepository,
        IVehicleRepository                vehicleRepository,
        ICustomerRepository               customerRepository,
        IFleetRepository                  fleetRepository,
        IWorkOrderApprovalTokenRepository tokenRepository,
        ICurrentUserService               currentUser,
        IPdfService                       pdfService,
        IEmailService                     emailService,
        IUnitOfWork                       unitOfWork,
        IMapper                           mapper,
        IApprovalLinkBuilder              approvalLinkBuilder,
        ILogger<ChangeWorkOrderStatusCommandHandler> logger)
    {
        _workOrderRepository = workOrderRepository;
        _vehicleRepository   = vehicleRepository;
        _customerRepository  = customerRepository;
        _fleetRepository     = fleetRepository;
        _tokenRepository     = tokenRepository;
        _currentUser         = currentUser;
        _pdfService          = pdfService;
        _emailService        = emailService;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
        _approvalLinkBuilder = approvalLinkBuilder;
        _logger              = logger;
    }

    public async Task<WorkOrderDetailDto> Handle(ChangeWorkOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), request.WorkOrderId);

        try
        {
            workOrder.ChangeStatus(request.NewStatus, _currentUser.UserId, request.Note);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        _workOrderRepository.Update(workOrder);

        string? approvalLink = null;
        if (workOrder.CurrentStatus == WorkOrderStatus.AwaitingApproval)
            approvalLink = await GenerateApprovalTokenAsync(workOrder.Id, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<WorkOrderDetailDto>(workOrder);

        // Todos los datos de DB se obtienen DENTRO del scope del request,
        // antes de que el contexto se disponga. El fire-and-forget solo recibe valores.
        if (workOrder.CurrentStatus == WorkOrderStatus.AwaitingApproval)
            await TryEnqueueQuoteEmailAsync(workOrder, dto, approvalLink!, cancellationToken);

        return dto;
    }

    private async Task TryEnqueueQuoteEmailAsync(
        WorkOrder workOrder, WorkOrderDetailDto dto, string approvalLink,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(workOrder.VehicleId, cancellationToken);
        if (vehicle is null) return;

        string recipientName, recipientEmail;

        if (workOrder.CustomerIdAtEntry.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(workOrder.CustomerIdAtEntry.Value, cancellationToken);
            if (customer is null) return;
            recipientName  = $"{customer.FirstName} {customer.LastName}";
            recipientEmail = customer.Email ?? string.Empty;
            if (string.IsNullOrEmpty(recipientEmail)) return;
        }
        else if (workOrder.FleetIdAtEntry.HasValue)
        {
            var contact = await _customerRepository.GetByFleetIdAsync(workOrder.FleetIdAtEntry.Value, cancellationToken);
            if (contact is null) return;
            recipientName  = $"{contact.FirstName} {contact.LastName}";
            recipientEmail = contact.Email ?? string.Empty;
            if (string.IsNullOrEmpty(recipientEmail)) return;
        }
        else return;

        // Capturamos todos los valores necesarios — el lambda no cierra sobre ningún servicio scoped
        var brand         = vehicle.Brand;
        var model         = vehicle.Model;
        var licensePlate  = vehicle.LicensePlate;
        var year          = vehicle.Year;
        var totalAmount   = dto.TotalAmount;
        var name          = recipientName;
        var email         = recipientEmail;

        var pdfData = new QuotePdfData(
            WorkOrder:      dto,
            LicensePlate:   licensePlate,
            VehicleBrand:   brand,
            VehicleModel:   model,
            VehicleYear:    year,
            RecipientName:  name,
            RecipientEmail: email);

        var pdfBytes = _pdfService.GenerateQuotePdf(pdfData);

        // Fire-and-forget: solo IEmailService (singleton/transient-safe) y valores primitivos
        _ = SendEmailAsync(email, brand, model, licensePlate, name, totalAmount, approvalLink, pdfBytes);
    }

    private async Task SendEmailAsync(
        string to, string brand, string model, string licensePlate,
        string recipientName, decimal totalAmount, string approvalLink, byte[] pdfBytes)
    {
        try
        {
            await _emailService.SendAsync(
                to:             to,
                subject:        $"Presupuesto para su vehículo {brand} {model} — MyCarApp",
                htmlBody:       BuildQuoteEmailBody(recipientName, brand, model, totalAmount, approvalLink),
                attachment:     pdfBytes,
                attachmentName: $"Presupuesto-{licensePlate}-{DateTime.UtcNow:yyyyMMdd}.pdf",
                cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email de presupuesto a {Email}", to);
        }
    }

    private async Task<string> GenerateApprovalTokenAsync(Guid workOrderId, CancellationToken cancellationToken)
    {
        var existing = await _tokenRepository.GetActiveByWorkOrderIdAsync(workOrderId, cancellationToken);
        if (existing is not null)
            _tokenRepository.Delete(existing);

        var tokenValue = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        await _tokenRepository.AddAsync(new WorkOrderApprovalToken
        {
            WorkOrderId = workOrderId,
            Token       = tokenValue,
            ExpiresAt   = DateTime.UtcNow.AddHours(48),
            IsUsed      = false,
        }, cancellationToken);

        return _approvalLinkBuilder.Build(tokenValue);
    }

    private static string BuildQuoteEmailBody(
        string name, string brand, string model, decimal total, string approvalLink) => $"""
        <h2>Hola, {name}!</h2>
        <p>El diagnóstico de tu <strong>{brand} {model}</strong> está listo.</p>
        <p>Adjuntamos el presupuesto detallado. El total estimado es de <strong>$ {total:N0}</strong>.</p>
        <p>Para autorizar el trabajo, hacé clic en el siguiente botón:</p>
        <p style="margin:24px 0;">
          <a href="{approvalLink}"
             style="background:#1d4ed8;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:bold;display:inline-block;margin-right:12px;">
            Aprobar presupuesto
          </a>
          <a href="https://wa.me/WHATSAPP_NUMBER_PLACEHOLDER"
             style="background:#25D366;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:bold;display:inline-block;">
            Contactar por WhatsApp
          </a>
        </p>
        <p style="color:#6b7280;font-size:0.875rem;">
          Este enlace es válido por 48 horas. Si no solicitaste este presupuesto, ignorá este mensaje.
        </p>
        <br>
        <p><em>MyCarApp — Taller de Servicios Automotores</em></p>
        """;
}
