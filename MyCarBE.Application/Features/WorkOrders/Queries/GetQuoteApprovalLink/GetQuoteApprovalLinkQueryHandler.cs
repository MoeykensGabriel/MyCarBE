using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetQuoteApprovalLink;

/// <summary>
/// Devuelve el link de aprobación vigente del presupuesto.
///
/// Existe porque el link se arma dentro de SendQuote y muere en el email: sin esto,
/// la oficina no tiene forma de volver a pasárselo al cliente por WhatsApp. Solo
/// lee el token que ya está guardado — no genera uno nuevo ni toca la orden, así
/// que reenviar no invalida el link que el cliente pueda tener.
/// </summary>
public class GetQuoteApprovalLinkQueryHandler : IRequestHandler<GetQuoteApprovalLinkQuery, QuoteApprovalLinkDto>
{
    private readonly IWorkOrderApprovalTokenRepository _tokenRepository;
    private readonly IApprovalLinkBuilder              _approvalLinkBuilder;

    public GetQuoteApprovalLinkQueryHandler(
        IWorkOrderApprovalTokenRepository tokenRepository,
        IApprovalLinkBuilder              approvalLinkBuilder)
    {
        _tokenRepository     = tokenRepository;
        _approvalLinkBuilder = approvalLinkBuilder;
    }

    public async Task<QuoteApprovalLinkDto> Handle(GetQuoteApprovalLinkQuery request, CancellationToken cancellationToken)
    {
        // GetActiveByWorkOrderIdAsync ya filtra usado / borrado / vencido.
        var token = await _tokenRepository.GetActiveByWorkOrderIdAsync(request.WorkOrderId, cancellationToken);

        return token is null
            ? new QuoteApprovalLinkDto(null, null)
            : new QuoteApprovalLinkDto(_approvalLinkBuilder.Build(token.Token), token.ExpiresAt);
    }
}
