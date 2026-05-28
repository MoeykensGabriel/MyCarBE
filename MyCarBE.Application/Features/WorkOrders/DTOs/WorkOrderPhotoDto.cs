using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

public record WorkOrderPhotoDto(
    Guid      Id,
    PhotoType PhotoType,
    string    Url,
    string?   Caption,
    DateTime  TakenAt,
    /// <summary>
    /// Si está presente, la foto pertenece a ese servicio puntual de la orden
    /// (subida por el mecánico al cerrar el servicio). Si es null y InspectionReportId
    /// también es null, es una foto general del vehículo (Before del intake o After
    /// de la entrega).
    /// </summary>
    Guid?     WorkOrderServiceId,
    /// <summary>
    /// Si está presente, la foto pertenece a un reporte de inspección puntual
    /// (subida por el mecánico durante la inspección inicial).
    /// </summary>
    Guid?     InspectionReportId
);
