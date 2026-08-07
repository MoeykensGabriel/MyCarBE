using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.PromoteInspectionToWorkOrder;

/// <summary>
/// El cliente aceptó arreglar lo que la inspección encontró: la orden de solo inspección
/// se convierte en orden de trabajo y vuelve a cotización (Completed → Diagnosing).
///
/// Endpoint dedicado y no un cambio de estado genérico porque además de mover el estado hay
/// que marcar PromotedToRepairAt — sin esa marca la orden quedaría en Diagnosing pero seguiría
/// contando como solo inspección, y no aceptaría que le carguen trabajo.
///
/// Los hallazgos y propuestas de la inspección siguen colgando de la misma orden: no se
/// re-inspecciona nada y no se convierte nada automáticamente. La oficina elige qué propuestas
/// pasan al presupuesto con ConvertInspectionProposals, igual que en cualquier orden.
/// </summary>
public record PromoteInspectionToWorkOrderCommand(
    Guid    WorkOrderId,
    string? Note = null
) : IRequest<WorkOrderDetailDto>;
