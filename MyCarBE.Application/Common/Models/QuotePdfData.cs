using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Common.Models;

/// <summary>
/// Datos consolidados para generar el PDF del presupuesto.
/// El handler lo arma y lo pasa a IPdfService.
/// </summary>
public record QuotePdfData(
    WorkOrderDetailDto WorkOrder,
    string             LicensePlate,
    string             VehicleBrand,
    string             VehicleModel,
    int                VehicleYear,
    string             RecipientName,
    string             RecipientEmail
);
