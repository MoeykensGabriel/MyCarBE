using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ConvertInspectionProposals;

/// <summary>
/// Convierte propuestas de inspección (servicios + repuestos sugeridos por los mecánicos)
/// en items reales del presupuesto de la orden.
///
/// El admin elige qué propuestas pasan al presupuesto (subset). Solo válido en Diagnosing.
/// Cada propuesta seleccionada se traduce en un WorkOrderService o WorkOrderPart, copiando
/// los datos del mecánico. Luego el admin puede editar precios/cantidades con los endpoints existentes.
///
/// Una propuesta se puede convertir múltiples veces — el sistema no marca la propuesta como
/// "ya convertida". El admin es responsable de no duplicar (la UI evita confusión).
/// </summary>
public record ConvertInspectionProposalsCommand(
    Guid              WorkOrderId,
    IReadOnlyList<Guid> ProposedServiceIds,
    IReadOnlyList<Guid> ProposedPartIds
) : IRequest<WorkOrderDetailDto>;
