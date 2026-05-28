using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.CloseInspection;

/// <summary>
/// El admin cierra la fase de inspección colectiva y la orden pasa a Diagnosing (cotización).
/// Requiere que TODAS las áreas activas tengan un reporte (de mecánico o marcado "sin hallazgos").
/// </summary>
public record CloseInspectionCommand(Guid WorkOrderId) : IRequest<WorkOrderDetailDto>;
