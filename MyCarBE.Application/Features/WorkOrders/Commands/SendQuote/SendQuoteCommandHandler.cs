using System.Security.Cryptography;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.SendQuote;

public class SendQuoteCommandHandler : IRequestHandler<SendQuoteCommand, WorkOrderDetailDto>
{
    /// <summary>TTL del presupuesto: 30 días desde el envío.</summary>
    private static readonly TimeSpan QuoteTtl = TimeSpan.FromDays(30);

    private readonly IWorkOrderRepository              _workOrderRepository;
    private readonly IWorkOrderApprovalTokenRepository _tokenRepository;
    private readonly ICurrentUserService               _currentUser;
    private readonly IQuoteDeliveryService             _quoteDelivery;
    private readonly IApprovalLinkBuilder              _approvalLinkBuilder;
    private readonly IUnitOfWork                       _unitOfWork;
    private readonly IMapper                           _mapper;
    private readonly ILogger<SendQuoteCommandHandler>  _logger;

    public SendQuoteCommandHandler(
        IWorkOrderRepository              workOrderRepository,
        IWorkOrderApprovalTokenRepository tokenRepository,
        ICurrentUserService               currentUser,
        IQuoteDeliveryService             quoteDelivery,
        IApprovalLinkBuilder              approvalLinkBuilder,
        IUnitOfWork                       unitOfWork,
        IMapper                           mapper,
        ILogger<SendQuoteCommandHandler>  logger)
    {
        _workOrderRepository = workOrderRepository;
        _tokenRepository     = tokenRepository;
        _currentUser         = currentUser;
        _quoteDelivery       = quoteDelivery;
        _approvalLinkBuilder = approvalLinkBuilder;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
        _logger              = logger;
    }

    public async Task<WorkOrderDetailDto> Handle(SendQuoteCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // ── Validaciones de negocio ──────────────────────────────────────────────
        if (workOrder.CurrentStatus != WorkOrderStatus.Diagnosing)
            throw new BadRequestException(
                $"El presupuesto solo se puede enviar desde 'Diagnosing'. Estado actual: '{workOrder.CurrentStatus}'.");

        var activeServices = workOrder.Services.Where(s => !s.IsDeleted).ToList();
        var activeParts    = workOrder.Parts.Where(p => !p.IsDeleted).ToList();

        if (activeServices.Count == 0 && activeParts.Count == 0)
            throw new BadRequestException(
                "El presupuesto debe tener al menos un servicio o repuesto cargado.");

        ValidateAlternativeGroups(activeServices, activeParts);

        // ── Cambios atómicos (mismo SaveChanges) ────────────────────────────────
        var now = DateTime.UtcNow;

        foreach (var s in activeServices) s.FrozenAt = now;
        foreach (var p in activeParts)    p.FrozenAt = now;

        workOrder.QuoteExpiresAt = now.Add(QuoteTtl);

        try
        {
            workOrder.ChangeStatus(WorkOrderStatus.AwaitingApproval, _currentUser.UserId,
                note: "Presupuesto enviado al cliente.");
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        var approvalLink = await GenerateApprovalTokenAsync(workOrder.Id, now, cancellationToken);

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<WorkOrderDetailDto>(workOrder);

        // ── Email fire-and-forget ────────────────────────────────────────────────
        // Cualquier fallo de SMTP no rompe la operación: el admin ve la orden enviada
        // y puede reenviar el link manualmente si es necesario.
        try
        {
            await _quoteDelivery.SendQuoteEmailAsync(workOrder, dto, approvalLink, cancellationToken);
        }
        catch (Exception ex)
        {
            // Captura defensiva: si IQuoteDeliveryService falla síncronamente (la parte
            // de lectura de DB), no rompemos la respuesta. El fire-and-forget interno
            // ya maneja sus propios errores.
            _logger.LogError(ex, "Error preparando email de presupuesto para WO {WorkOrderId}", workOrder.Id);
        }

        return dto;
    }

    /// <summary>
    /// Cada grupo de alternativas debe tener al menos 2 items — un grupo de 1 no tiene sentido
    /// (no hay nada para elegir). Esto rechaza errores de carga del admin antes de mandarle el
    /// link al cliente.
    /// </summary>
    private static void ValidateAlternativeGroups(
        IReadOnlyList<WorkOrderService> services,
        IReadOnlyList<WorkOrderPart>    parts)
    {
        var groupCounts = new Dictionary<Guid, int>();

        foreach (var s in services)
            if (s.AlternativeGroupId.HasValue)
                groupCounts[s.AlternativeGroupId.Value] = groupCounts.GetValueOrDefault(s.AlternativeGroupId.Value) + 1;

        foreach (var p in parts)
            if (p.AlternativeGroupId.HasValue)
                groupCounts[p.AlternativeGroupId.Value] = groupCounts.GetValueOrDefault(p.AlternativeGroupId.Value) + 1;

        var brokenGroups = groupCounts.Where(kv => kv.Value < 2).Select(kv => kv.Key).ToList();
        if (brokenGroups.Count > 0)
            throw new BadRequestException(
                "Algunos grupos de alternativas tienen un solo item. Cada grupo debe tener al menos 2 opciones para que el cliente pueda elegir.");
    }

    private async Task<string> GenerateApprovalTokenAsync(Guid workOrderId, DateTime now, CancellationToken cancellationToken)
    {
        // Invalida cualquier token previo activo — si el admin reenvía, el link viejo deja de funcionar
        var existing = await _tokenRepository.GetActiveByWorkOrderIdAsync(workOrderId, cancellationToken);
        if (existing is not null)
            _tokenRepository.Delete(existing);

        // 32 bytes random = 64 chars hex
        var tokenValue = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        await _tokenRepository.AddAsync(new WorkOrderApprovalToken
        {
            WorkOrderId = workOrderId,
            Token       = tokenValue,
            ExpiresAt   = now.Add(QuoteTtl),
            IsUsed      = false,
        }, cancellationToken);

        return _approvalLinkBuilder.Build(tokenValue);
    }
}
