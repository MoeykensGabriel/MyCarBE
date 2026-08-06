using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Common.Models;

/// <summary>
/// Datos consolidados para el INFORME DE CIERRE: el documento que el taller le entrega al
/// cliente cuando la orden termina, para que quede escrito todo lo que pasó con el vehículo.
///
/// Se diferencia de <see cref="QuotePdfData"/> en el propósito: aquel es la cotización
/// PREVIA (lo que se va a hacer y cuánto sale), este es el comprobante POSTERIOR (lo que
/// se inspeccionó, lo que se encontró y lo que efectivamente se hizo).
///
/// Por eso trae los reportes de inspección completos, que el presupuesto no necesita.
/// </summary>
public record OrderClosingPdfData(
    WorkOrderDetailDto WorkOrder,

    // ── Vehículo ────────────────────────────────────────────────────────────
    string  LicensePlate,
    string  VehicleBrand,
    string  VehicleModel,
    int     VehicleYear,
    string? VehicleColor,
    string? VehicleVin,

    // ── Contacto ────────────────────────────────────────────────────────────
    /// <summary>Cliente particular o razón social de la flota.</summary>
    string  OwnerName,
    /// <summary>"Cliente" o "Flota" — qué tipo de titular es, para rotular el bloque.</summary>
    string  OwnerKind,
    string? OwnerPhone,
    string? OwnerEmail,
    /// <summary>DNI del cliente o CUIT de la flota. Null si no se cargó.</summary>
    string? OwnerDocument,

    /// <summary>
    /// Reportes de inspección de la orden, con hallazgos, propuestas y fotos. Es el corazón
    /// del informe: es lo que el cliente no tiene forma de ver una vez que se lleva el auto.
    /// </summary>
    IReadOnlyList<InspectionReportDto> InspectionReports,

    /// <summary>Momento de emisión del informe — va en el encabezado.</summary>
    DateTime GeneratedAt,

    /// <summary>
    /// Versión INTERNA del taller: muestra todo lo que la del cliente deja afuera — quién
    /// revisó cada área, quién hizo cada servicio, precios unitarios, códigos de repuesto,
    /// ítems rechazados y la línea de tiempo completa con sus notas.
    ///
    /// El documento avisa en el encabezado que no es para entregar.
    /// </summary>
    bool Internal = false
);
