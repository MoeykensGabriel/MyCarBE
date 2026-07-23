namespace MyCarBE.Application.Features.WorkOrders.DTOs;

/// <summary>
/// Link de aprobación vigente de un presupuesto, para poder reenviárselo al cliente
/// por otro canal (WhatsApp) sin volver a enviar el presupuesto.
///
/// Ambos campos vienen en null cuando la orden no tiene un token activo: nunca se
/// envió el presupuesto, venció, o el cliente ya lo usó para decidir. No es un
/// error — es la respuesta legítima a "¿hay un link para reenviar?".
/// </summary>
public record QuoteApprovalLinkDto(
    string?   ApprovalLink,
    DateTime? ExpiresAt
);
