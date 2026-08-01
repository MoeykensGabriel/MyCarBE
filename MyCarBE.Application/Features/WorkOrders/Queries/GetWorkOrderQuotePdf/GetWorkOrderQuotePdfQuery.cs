using MediatR;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderQuotePdf;

/// <summary>
/// PDF del presupuesto más el número de orden, para que el archivo descargado se llame
/// "Presupuesto-1042.pdf" y no con un pedazo del Guid. El cliente guarda ese archivo y
/// después lo menciona por teléfono: tiene que coincidir con lo que ve en pantalla.
/// </summary>
public record QuotePdfResult(byte[] Content, int OrderNumber);

public record GetWorkOrderQuotePdfQuery(Guid WorkOrderId) : IRequest<QuotePdfResult>;
