using MediatR;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderClosingPdf;

/// <summary>
/// PDF del informe de cierre más el número de orden, para que el archivo se llame
/// "Informe-1042.pdf". Mismo criterio que el presupuesto: el cliente guarda el archivo y
/// después lo menciona por teléfono, así que el nombre tiene que coincidir con lo que ve
/// en pantalla (nunca un pedazo del Guid).
/// </summary>
public record ClosingPdfResult(byte[] Content, int OrderNumber);

/// <summary>
/// Genera el informe de cierre de una orden ya terminada: todo lo que pasó con el vehículo
/// en esta visita.
/// </summary>
/// <param name="Internal">
/// true = versión INTERNA del taller: el mismo relato pero sin recortes — quién revisó cada
/// área, quién hizo cada servicio, precios unitarios, códigos de repuesto, ítems rechazados
/// y la línea de tiempo completa. Solo Admin.
///
/// false = versión que se le entrega al CLIENTE.
/// </param>
public record GetWorkOrderClosingPdfQuery(Guid WorkOrderId, bool Internal = false)
    : IRequest<ClosingPdfResult>;
