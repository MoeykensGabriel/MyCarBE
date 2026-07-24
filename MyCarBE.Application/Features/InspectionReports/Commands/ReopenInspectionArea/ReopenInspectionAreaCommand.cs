using MediatR;

namespace MyCarBE.Application.Features.InspectionReports.Commands.ReopenInspectionArea;

/// <summary>
/// Deshace una marca de la oficina sobre un área ("sin novedades" o "postergada"),
/// dejando el área otra vez pendiente. Es la vía para corregir un click por error:
/// borra (soft-delete) el InspectionReport de esa (orden, área). Solo aplica mientras
/// la orden sigue en inspección, y solo sobre marcas de oficina — los reportes de los
/// mecánicos los edita el propio mecánico.
/// </summary>
public record ReopenInspectionAreaCommand(
    Guid WorkOrderId,
    Guid AreaId
) : IRequest<Unit>;
