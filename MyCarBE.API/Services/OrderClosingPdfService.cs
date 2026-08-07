using MyCarBE.Application.Common.Formatting;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MyCarBE.API.Services;

/// <summary>
/// INFORME DE CIERRE: el documento que el taller le entrega al cliente cuando la orden
/// termina. Cuenta la visita completa en orden cronológico — con qué entró el vehículo, qué
/// se revisó, qué se encontró y qué se hizo — para que el cliente se lleve por escrito algo
/// que hoy solo se cuenta por teléfono.
///
/// Dos variantes del mismo documento, según la orden:
///   - Orden de trabajo → incluye el detalle de lo realizado y el total.
///   - Solo inspección  → sin total ni trabajo realizado (no hubo), pero con el resultado de
///     la inspección y el trabajo SUGERIDO, aclarando que no es un presupuesto.
/// </summary>
public class OrderClosingPdfService : IOrderClosingPdfService
{
    // Paleta: la misma del PDF de presupuesto, para que los dos documentos se vean de la
    // misma familia. El acento cambia a verde: este informe cierra, no pide aprobación.
    private const string Ink        = "#1a1a2e";
    private const string Accent     = "#2a9d5c";
    // Bordó para la versión interna: a simple vista, y también fotocopiado, no se confunde
    // con el informe que se le entrega al cliente.
    private const string InternalAccent = "#8b1e3f";
    private const string Muted      = "#777777";
    private const string SoftBg     = "#f8f9fa";
    private const string BorderGray = "#dddddd";

    public byte[] GenerateClosingReport(OrderClosingPdfData data)
        => BuildDocument(data).GeneratePdf();

    /// <summary>
    /// Arma el documento sin renderizarlo. Separado de GenerateClosingReport para poder
    /// rasterizarlo a imagen y revisar el layout — QuestPDF valida la maqueta recién al
    /// generar, así que un error de diseño no lo ve el compilador.
    /// </summary>
    public static Document BuildDocument(OrderClosingPdfData data)
    {
        var wo = data.WorkOrder;
        var isInspectionOnly = wo.IsInspectionOnly;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                ComposeHeader(page, data, isInspectionOnly);

                page.Content().PaddingTop(16).Column(col =>
                {
                    ComposeVehicleAndContact(col, data);
                    ComposeIntakeReason(col, data);
                    ComposeInspection(col, data);

                    if (isInspectionOnly)
                        ComposeSuggestedWork(col, data);
                    else
                        ComposeWorkDone(col, data);

                    ComposeClosing(col, data, isInspectionOnly);
                });

                ComposeFooter(page);
            });
        });
    }

    // ─── Encabezado / pie ────────────────────────────────────────────────────

    private static void ComposeHeader(PageDescriptor page, OrderClosingPdfData data, bool isInspectionOnly)
    {
        var wo = data.WorkOrder;

        var isInternal = data.Internal;
        var accent     = isInternal ? InternalAccent : Accent;

        var title = isInternal
            ? (isInspectionOnly ? "INSPECCIÓN — INTERNO" : "CIERRE — INTERNO")
            : (isInspectionOnly ? "INFORME DE INSPECCIÓN" : "INFORME DE CIERRE");

        page.Header().Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("GB Service").Bold().FontSize(22).FontColor(Ink);
                    c.Item().Text("Taller de Servicios Automotores").FontSize(11).FontColor("#555");
                });

                row.ConstantItem(190).Column(c =>
                {
                    c.Item().AlignRight().Text(title).Bold().FontSize(14).FontColor(accent);
                    c.Item().AlignRight().Text($"Orden Nº {wo.Number}").Bold().FontSize(10).FontColor(Ink);
                    c.Item().AlignRight()
                        .Text($"Emitido: {data.GeneratedAt:dd/MM/yyyy HH:mm} hs").FontSize(9).FontColor(Muted);
                });
            });

            col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(accent);

            // La advertencia va en el ENCABEZADO y no una vez al principio: el informe interno
            // se imprime y se separa, y cualquier hoja suelta tiene que gritar que no se
            // entrega. Es la única defensa contra que termine en manos del cliente.
            if (isInternal)
                col.Item().PaddingTop(4).Background(InternalAccent).Padding(4).AlignCenter()
                    .Text("DOCUMENTO INTERNO — NO ENTREGAR AL CLIENTE")
                    .Bold().FontSize(8).FontColor(Colors.White);
        });
    }

    private static void ComposeFooter(PageDescriptor page)
    {
        page.Footer().PaddingTop(8).Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor("#cccccc");
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem()
                    .Text("GB Service — Sistema de Gestión de Taller")
                    .FontSize(8).FontColor("#aaaaaa");

                row.ConstantItem(90).AlignRight().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(8).FontColor("#aaaaaa"));
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        });
    }

    // ─── 1. Vehículo y contacto ──────────────────────────────────────────────

    private static void ComposeVehicleAndContact(ColumnDescriptor col, OrderClosingPdfData data)
    {
        SectionTitle(col, "1. DATOS DEL VEHÍCULO Y DEL CONTACTO", data.Internal);

        col.Item().Row(row =>
        {
            row.RelativeItem().Background(SoftBg).Padding(10).Column(c =>
            {
                c.Item().Text("VEHÍCULO").Bold().FontSize(8).FontColor(Muted);
                c.Item().PaddingTop(4)
                    .Text($"{data.VehicleBrand} {data.VehicleModel} {data.VehicleYear}".Trim())
                    .Bold().FontSize(12);
                Field(c, "Patente", data.LicensePlate);
                Field(c, "Color", data.VehicleColor);
                Field(c, "Nº de chasis", data.VehicleVin);
                Field(c, "Km al ingreso", MoneyFormat.ArNumber(data.WorkOrder.MileageAtEntry) + " km");
            });

            row.ConstantItem(12);

            row.RelativeItem().Background(SoftBg).Padding(10).Column(c =>
            {
                c.Item().Text(data.OwnerKind.ToUpperInvariant()).Bold().FontSize(8).FontColor(Muted);
                c.Item().PaddingTop(4).Text(data.OwnerName).Bold().FontSize(12);
                Field(c, data.OwnerKind == "Flota" ? "CUIT" : "Documento", data.OwnerDocument);
                Field(c, "Teléfono", data.OwnerPhone);
                Field(c, "Email", data.OwnerEmail);
                // Solo en flotas: quién trajo físicamente el vehículo. En un particular es
                // el titular y repetirlo sería ruido.
                Field(c, "Trajo el vehículo", data.WorkOrder.ContactPersonName);
                Field(c, "Tel. de contacto", data.WorkOrder.ContactPersonPhone);
            });
        });

        // Datos administrativos de la orden: en el informe del cliente no pintan nada, pero
        // son lo primero que busca la oficina cuando revisa una orden vieja.
        if (data.Internal)
        {
            var wo = data.WorkOrder;

            col.Item().PaddingTop(10).Background(SoftBg).Padding(10).Column(c =>
            {
                c.Item().Text("DATOS DE LA ORDEN").Bold().FontSize(8).FontColor(Muted);
                Field(c, "Estado", StatusLabel(wo.CurrentStatus));
                Field(c, "Tipo de ingreso", wo.Purpose == WorkOrderPurpose.InspectionOnly
                    ? "Solo inspección" + (wo.PromotedToRepairAt is { } p
                        ? $" — promovida a orden de trabajo el {p:dd/MM/yyyy}"
                        : "")
                    : "Orden de trabajo");
                Field(c, "Condición de venta", wo.SaleCondition?.ToString());
                Field(c, "Orden de compra", wo.PurchaseOrderNumber);
                Field(c, "Seña", wo.DepositAmount is { } d ? MoneyFormat.ArCurrency(d) : null);
                Field(c, "Venc. presupuesto", wo.QuoteExpiresAt is { } q ? $"{q:dd/MM/yyyy}" : null);
                Field(c, "Duración estimada", DurationFormat.ArDuration(wo.TotalEstimatedMinutes));
                Field(c, "Agendado", wo.ScheduledStart is { } s
                    ? $"{s:dd/MM/yyyy HH:mm} hs" + (wo.ScheduledEnd is { } e ? $" — {e:HH:mm} hs" : "")
                    : null);
            });
        }
    }

    // ─── 2. Motivo del ingreso ───────────────────────────────────────────────

    private static void ComposeIntakeReason(ColumnDescriptor col, OrderClosingPdfData data)
    {
        var wo = data.WorkOrder;

        SectionTitle(col, "2. MOTIVO DEL INGRESO", data.Internal);

        col.Item().Text($"Ingreso: {wo.CreatedAt:dd/MM/yyyy HH:mm} hs").FontSize(9).FontColor(Muted);

        if (!string.IsNullOrWhiteSpace(wo.ServiceReason))
            col.Item().PaddingTop(6).Text(wo.ServiceReason).FontSize(11);
        else
            col.Item().PaddingTop(6).Text("No se registró un motivo de ingreso.").Italic().FontColor(Muted);

        if (!string.IsNullOrWhiteSpace(wo.CustomerNote))
        {
            col.Item().PaddingTop(8).Text("Lo que nos contó el cliente").Bold().FontSize(8).FontColor(Muted);
            col.Item().PaddingTop(2).Text(wo.CustomerNote).Italic().FontColor("#444444");
        }
    }

    // ─── 3. Inspección ───────────────────────────────────────────────────────

    private static void ComposeInspection(ColumnDescriptor col, OrderClosingPdfData data)
    {
        SectionTitle(col, "3. INSPECCIÓN DEL VEHÍCULO", data.Internal);

        if (data.InspectionReports.Count == 0)
        {
            col.Item().Text("No se registraron reportes de inspección en esta orden.")
                .Italic().FontColor(Muted);
            return;
        }

        col.Item().Text(
                "Detalle de lo que se revisó, área por área. Las áreas marcadas como postergadas " +
                "no llegaron a inspeccionarse en esta visita y quedan pendientes para la próxima.")
            .FontSize(9).FontColor(Muted);

        foreach (var report in data.InspectionReports)
            ComposeInspectionArea(col, report, data.Internal);
    }

    private static void ComposeInspectionArea(ColumnDescriptor col, InspectionReportDto report, bool isInternal)
    {
        var (label, color) = InspectionOutcome(report);

        // La ficha de un área se lee como una unidad: sin esto el corte de página deja el
        // recuadro de fotos solo arriba de la hoja siguiente, sin el título del área.
        // PreventPageBreak (y no ShowEntire) porque degrada en vez de tirar excepción si un
        // hallazgo muy largo no entra en una página.
        col.Item().PaddingTop(10).PreventPageBreak().Border(1).BorderColor(BorderGray).Padding(10).Column(c =>
        {
            c.Item().Row(row =>
            {
                row.RelativeItem().Text(report.AreaName).Bold().FontSize(11).FontColor(Ink);
                row.ConstantItem(150).AlignRight().Text(label).Bold().FontSize(9).FontColor(color);
            });

            // El nombre del mecánico SOLO en la versión interna: al cliente le importa QUÉ se
            // encontró, no QUIÉN lo miró, y ponerlo por escrito expone al mecánico ante un
            // reclamo posterior. Puertas adentro es justamente el dato que se busca.
            if (isInternal)
                c.Item().PaddingTop(2)
                    .Text(string.IsNullOrWhiteSpace(report.MechanicFullName)
                        ? "Reportado por la oficina"
                        : $"Revisado por {report.MechanicFullName}")
                    .FontSize(8).FontColor(Muted);

            // Un reporte tardío llegó después de cerrada la inspección inicial. Decirlo evita
            // que el cliente lea una contradicción entre las fechas del informe.
            if (report.IsLate)
                c.Item().PaddingTop(2)
                    .Text("Revisión complementaria — se realizó después de la inspección inicial.")
                    .Italic().FontSize(8).FontColor(Muted);

            if (!string.IsNullOrWhiteSpace(report.Findings))
                c.Item().PaddingTop(6).Text(report.Findings).FontSize(10);

            if (!string.IsNullOrWhiteSpace(report.SkipReason))
                c.Item().PaddingTop(6).Text($"Motivo: {report.SkipReason}").Italic().FontSize(9).FontColor("#444444");

            // Trabajo que el mecánico sugirió a partir de lo que vio. Sin importes: acá es el
            // relato de la inspección, y los precios de lo que efectivamente se hizo van en la
            // sección de trabajo realizado.
            if (report.ProposedServices.Count > 0 || report.ProposedParts.Count > 0)
            {
                c.Item().PaddingTop(6).Text("Trabajo sugerido por el mecánico")
                    .Bold().FontSize(8).FontColor(Muted);

                // La estimación del mecánico es material de trabajo, no un precio: puertas
                // afuera se muestra qué sugirió, no cuánto calculó que salía.
                foreach (var ps in report.ProposedServices)
                    c.Item().PaddingLeft(8).Text(isInternal && ps.EstimatedLaborCost > 0
                        ? $"• {ps.Name} — estimado {MoneyFormat.ArCurrency(ps.EstimatedLaborCost)}"
                        : $"• {ps.Name}").FontSize(9);

                // "Repuesto:" al frente porque si no un repuesto propuesto se lee como un
                // trabajo más de la lista.
                foreach (var pp in report.ProposedParts)
                    c.Item().PaddingLeft(8).Text(isInternal && pp.EstimatedUnitPrice is { } est
                        ? $"• Repuesto: {pp.Name} (x{pp.Quantity}) — estimado {MoneyFormat.ArCurrency(est)} c/u"
                          + (string.IsNullOrWhiteSpace(pp.ProductCode) ? "" : $" [{pp.ProductCode}]")
                        : $"• Repuesto: {pp.Name} (x{pp.Quantity})").FontSize(9);
            }

            if (isInternal)
                c.Item().PaddingTop(6)
                    .Text($"Reporte cargado el {report.CreatedAt:dd/MM/yyyy HH:mm} hs" +
                          (report.UpdatedAt > report.CreatedAt
                              ? $" · corregido el {report.UpdatedAt:dd/MM/yyyy HH:mm} hs"
                              : ""))
                    .FontSize(8).FontColor(Muted);

            PhotoPlaceholder(c, report, isInternal);
        });
    }

    /// <summary>
    /// Espacio reservado para las fotos del área. Todavía no se embeben (hay que resolver la
    /// descarga desde S3 al generar el PDF), pero el hueco ya queda maquetado: cuando las
    /// fotos entren, el documento no cambia de forma. Si el área tiene fotos cargadas lo
    /// avisa, así el cliente sabe que existen y puede verlas en el portal.
    /// </summary>
    private static void PhotoPlaceholder(ColumnDescriptor col, InspectionReportDto report, bool isInternal)
    {
        var count = report.Photos.Count;

        col.Item().PaddingTop(8).Background(SoftBg).Border(1).BorderColor(BorderGray)
            .Padding(12).AlignCenter().Column(c =>
            {
                c.Item().AlignCenter()
                    .Text(count > 0
                        ? $"[ Espacio reservado para {count} foto{(count > 1 ? "s" : "")} de esta área ]"
                        : "[ Espacio reservado para las fotos de esta área ]")
                    .Italic().FontSize(9).FontColor(Muted);

                if (count > 0)
                    c.Item().PaddingTop(2).AlignCenter()
                        .Text(isInternal
                            ? "Por ahora se ven en la ficha de la orden."
                            : "Por ahora podés verlas en el portal del cliente.")
                        .FontSize(8).FontColor(Muted);
            });
    }

    private static (string Label, string Color) InspectionOutcome(InspectionReportDto report)
    {
        if (report.IsSkipped)     return ("PENDIENTE DE REVISAR", "#b45309");
        if (report.HasIssue)      return ("CON HALLAZGOS",        "#c1121f");
        if (report.IsNoFindings)  return ("SIN NOVEDADES",        Accent);
        return ("REVISADO — SIN NOVEDADES", Accent);
    }

    // ─── 4a. Trabajo realizado (orden de trabajo) ────────────────────────────

    private static void ComposeWorkDone(ColumnDescriptor col, OrderClosingPdfData data)
    {
        var wo = data.WorkOrder;

        var isInternal = data.Internal;

        SectionTitle(col, "4. DIAGNÓSTICO Y TRABAJO REALIZADO", isInternal);

        // Lo rechazado por el cliente no se hizo: listarlo en el informe del cliente haría
        // parecer que sí. Puertas adentro se muestra igual, marcado — saber qué se ofreció y
        // el cliente no quiso es justamente lo que la oficina va a buscar después.
        var services = isInternal
            ? wo.Services.ToList()
            : wo.Services.Where(s => s.ApprovalStatus != QuoteItemApprovalStatus.Rejected).ToList();
        var parts = isInternal
            ? wo.Parts.ToList()
            : wo.Parts.Where(p => p.ApprovalStatus != QuoteItemApprovalStatus.Rejected).ToList();

        if (services.Count == 0 && parts.Count == 0)
        {
            col.Item().Text(
                    "La orden se cerró sin trabajos a realizar: la revisión no derivó en " +
                    "servicios ni repuestos para esta visita.")
                .FontSize(10);
            return;
        }

        if (services.Count > 0)
        {
            col.Item().Text("SERVICIOS REALIZADOS").Bold().FontSize(8).FontColor(Muted);

            if (isInternal)
            {
                foreach (var service in services)
                    ComposeInternalItem(
                        col,
                        name:      service.NameSnapshot,
                        quantity:  service.Quantity,
                        unitPrice: service.PriceSnapshot,
                        subtotal:  service.Subtotal,
                        status:    service.ApprovalStatus,
                        details:   BuildServiceDetails(service));
            }
            else
            {
                col.Item().PaddingTop(6).Table(table =>
                {
                    // Sin precio unitario: el taller le muestra al cliente el subtotal por
                    // línea y nada más.
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(35);
                        cols.RelativeColumn();
                        cols.ConstantColumn(90);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).AlignCenter()
                            .Text("Cant.").FontColor(Colors.White).Bold().FontSize(9);
                        h.Cell().Element(HeaderCell)
                            .Text("Servicio").FontColor(Colors.White).Bold().FontSize(9);
                        h.Cell().Element(HeaderCell).AlignRight()
                            .Text("Subtotal").FontColor(Colors.White).Bold().FontSize(9);
                    });

                    var isOdd = false;
                    foreach (var service in services)
                    {
                        var bg = isOdd ? SoftBg : "#ffffff";
                        isOdd = !isOdd;

                        IContainer Cell(IContainer c) => c.Background(bg).Padding(6);

                        table.Cell().Element(Cell).AlignCenter().Text(service.Quantity.ToString());
                        table.Cell().Element(Cell).Column(c =>
                        {
                            c.Item().Text(service.NameSnapshot).Bold();
                            // Lo que el mecánico observó mientras hacía el trabajo. Es la
                            // parte que le da valor al informe: explica POR QUÉ se hizo.
                            if (!string.IsNullOrWhiteSpace(service.MechanicFindings))
                                c.Item().PaddingTop(2)
                                    .Text(service.MechanicFindings).FontSize(9).FontColor("#444444");
                        });
                        table.Cell().Element(Cell).AlignRight()
                            .Text(MoneyFormat.ArCurrency(service.Subtotal)).Bold();
                    }
                });
            }
        }

        if (parts.Count > 0)
        {
            col.Item().PaddingTop(14);
            col.Item().Text("REPUESTOS UTILIZADOS").Bold().FontSize(8).FontColor(Muted);

            if (isInternal)
            {
                foreach (var part in parts)
                    ComposeInternalItem(
                        col,
                        name:      part.Name,
                        quantity:  part.Quantity,
                        unitPrice: part.UnitPrice,
                        subtotal:  part.Subtotal,
                        status:    part.ApprovalStatus,
                        details:   string.IsNullOrWhiteSpace(part.ProductCode)
                            ? new List<string>()
                            : new List<string> { $"Código: {part.ProductCode}" });
            }
            else
            {
                col.Item().PaddingTop(6).Table(table =>
                {
                    // Sin código de proveedor — es dato interno del taller.
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(35);
                        cols.RelativeColumn();
                        cols.ConstantColumn(90);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).AlignCenter()
                            .Text("Cant.").FontColor(Colors.White).Bold().FontSize(9);
                        h.Cell().Element(HeaderCell)
                            .Text("Repuesto").FontColor(Colors.White).Bold().FontSize(9);
                        h.Cell().Element(HeaderCell).AlignRight()
                            .Text("Subtotal").FontColor(Colors.White).Bold().FontSize(9);
                    });

                    var isOdd = false;
                    foreach (var part in parts)
                    {
                        var bg = isOdd ? SoftBg : "#ffffff";
                        isOdd = !isOdd;

                        IContainer Cell(IContainer c) => c.Background(bg).Padding(6);

                        table.Cell().Element(Cell).AlignCenter().Text(part.Quantity.ToString());
                        table.Cell().Element(Cell).Text(part.Name).Bold();
                        table.Cell().Element(Cell).AlignRight()
                            .Text(MoneyFormat.ArCurrency(part.Subtotal)).Bold();
                    }
                });
            }
        }

        col.Item().PaddingTop(8).AlignRight().Row(r =>
        {
            r.ConstantItem(180).Background(Ink).Padding(10).Row(inner =>
            {
                inner.RelativeItem().Text("TOTAL").FontColor(Colors.White).Bold().FontSize(13);
                inner.RelativeItem().AlignRight()
                    .Text(MoneyFormat.ArCurrency(wo.TotalAmount)).FontColor(Colors.White).Bold().FontSize(13);
            });
        });

        // En la interna la tabla lista ítems que NO suman al total (rechazados, y los
        // adicionales que el cliente todavía no decidió). Sin esta línea el total parece
        // mal sumado.
        if (isInternal)
            col.Item().PaddingTop(4).AlignRight()
                .Text("El total suma solo los ítems aprobados. Los rechazados y los adicionales " +
                      "pendientes de decisión se listan pero no computan.")
                .FontSize(8).FontColor(Muted);
    }

    /// <summary>
    /// Ficha de un ítem en la versión INTERNA. Va como ficha y no como fila de tabla porque
    /// acá cada ítem arrastra varias líneas de detalle (mecánico, fechas, notas), y una fila
    /// de tabla que no entra en la página se parte por columnas: el nombre queda en una hoja
    /// y los importes en la otra. Como ficha, PreventPageBreak la mueve entera.
    /// </summary>
    private static void ComposeInternalItem(
        ColumnDescriptor col,
        string name,
        int quantity,
        decimal unitPrice,
        decimal subtotal,
        QuoteItemApprovalStatus status,
        IReadOnlyList<string> details)
    {
        var (estado, colorEstado) = ApprovalLabel(status);

        col.Item().PaddingTop(6).PreventPageBreak()
            .Border(1).BorderColor(BorderGray).Padding(8).Column(c =>
            {
                c.Item().Row(row =>
                {
                    row.RelativeItem().Text(name).Bold().FontSize(10);
                    row.ConstantItem(70).AlignRight()
                        .Text(estado).Bold().FontSize(8).FontColor(colorEstado);
                });

                c.Item().PaddingTop(3).Row(row =>
                {
                    row.RelativeItem()
                        .Text($"{quantity} × {MoneyFormat.ArCurrency(unitPrice)}")
                        .FontSize(9).FontColor(Muted);
                    row.ConstantItem(100).AlignRight()
                        .Text(MoneyFormat.ArCurrency(subtotal)).Bold().FontSize(10);
                });

                foreach (var line in details)
                    c.Item().PaddingTop(2).Text(line).FontSize(8).FontColor(Muted);
            });
    }

    /// <summary>Líneas de detalle de un servicio para la ficha interna.</summary>
    private static List<string> BuildServiceDetails(WorkOrderServiceDto service)
    {
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(service.MechanicFindings))
            lines.Add(service.MechanicFindings);

        if (!string.IsNullOrWhiteSpace(service.AreaName))
            lines.Add($"Área: {service.AreaName}");

        lines.Add(string.IsNullOrWhiteSpace(service.AssignedMechanicName)
            ? "Sin mecánico asignado"
            : $"Ejecutó: {service.AssignedMechanicName}");

        if (service.CompletedAt is { } done)
            lines.Add($"Finalizado el {done:dd/MM/yyyy HH:mm} hs");

        // Nota interna del mecánico — el otro motivo de que este documento no se entregue.
        if (!string.IsNullOrWhiteSpace(service.MechanicNotes))
            lines.Add($"Nota: {service.MechanicNotes}");

        return lines;
    }

    /// <summary>
    /// Nombre del estado en castellano. El enum se escribe en inglés y el informe interno lo
    /// lee gente del taller: "AwaitingApproval" en una hoja impresa no le dice nada a nadie.
    /// </summary>
    private static string StatusLabel(WorkOrderStatus status) => status switch
    {
        WorkOrderStatus.Received         => "Recibida",
        WorkOrderStatus.UnderInspection  => "En inspección",
        WorkOrderStatus.Diagnosing       => "Diagnosticando",
        WorkOrderStatus.AwaitingApproval => "Esperando aprobación",
        WorkOrderStatus.Approved         => "Aprobada",
        WorkOrderStatus.InProgress       => "En progreso",
        WorkOrderStatus.Completed        => "Completada",
        WorkOrderStatus.Delivered        => "Entregada",
        WorkOrderStatus.Cancelled        => "Cancelada",
        _                                => status.ToString(),
    };

    /// <summary>Etiqueta y color de la decisión del cliente sobre un ítem (solo interna).</summary>
    private static (string Label, string Color) ApprovalLabel(QuoteItemApprovalStatus status)
        => status switch
        {
            QuoteItemApprovalStatus.Approved => ("APROBADO",  Accent),
            QuoteItemApprovalStatus.Rejected => ("RECHAZADO", "#c1121f"),
            _                                => ("PENDIENTE", "#b45309"),
        };

    // ─── 4b. Trabajo sugerido (solo inspección) ──────────────────────────────

    private static void ComposeSuggestedWork(ColumnDescriptor col, OrderClosingPdfData data)
    {
        SectionTitle(col, "4. TRABAJO SUGERIDO", data.Internal);

        var hasProposals = data.InspectionReports
            .Any(r => r.ProposedServices.Count > 0 || r.ProposedParts.Count > 0);

        if (!hasProposals)
        {
            col.Item().Text(
                    "La inspección no derivó en trabajos sugeridos: no se detectaron " +
                    "problemas que requieran intervención en este momento.")
                .FontSize(10);
            return;
        }

        col.Item().Text(
                "Esto es lo que recomendamos a partir de la inspección, agrupado por área. " +
                "El detalle de cada punto está en la sección anterior.")
            .FontSize(9).FontColor(Muted);

        foreach (var report in data.InspectionReports)
        {
            if (report.ProposedServices.Count == 0 && report.ProposedParts.Count == 0)
                continue;

            col.Item().PaddingTop(8).Text(report.AreaName).Bold().FontSize(10).FontColor(Ink);

            foreach (var ps in report.ProposedServices)
            {
                col.Item().PaddingLeft(10).Text($"• {ps.Name}").FontSize(10);
                if (!string.IsNullOrWhiteSpace(ps.Description))
                    col.Item().PaddingLeft(18).Text(ps.Description).FontSize(9).FontColor("#444444");
            }

            foreach (var pp in report.ProposedParts)
                col.Item().PaddingLeft(10).Text($"• Repuesto: {pp.Name} (x{pp.Quantity})").FontSize(10);
        }

        // Sin esta aclaración el listado se lee como un presupuesto cerrado, y no lo es:
        // las estimaciones del mecánico no pasaron por la oficina. El texto cambia según a
        // quién le habla la hoja — en la interna, tutear al lector no tiene sentido.
        col.Item().PaddingTop(12).PreventPageBreak().Background("#fff3cd").Padding(10).Column(c =>
        {
            c.Item().Text("Este informe NO es un presupuesto.").Bold().FontSize(9).FontColor("#856404");
            c.Item().PaddingTop(3)
                .Text(data.Internal
                    ? "Los importes estimados de la sección anterior son del mecánico y no " +
                      "pasaron por la oficina. Para cotizar, promové la orden a orden de trabajo " +
                      "y volcá las propuestas al presupuesto."
                    : "Si querés avanzar con alguno de estos trabajos, pedinos el presupuesto " +
                      "correspondiente y te lo enviamos con los importes al día.")
                .FontSize(9).FontColor("#856404");
        });
    }

    // ─── 5. Cierre ───────────────────────────────────────────────────────────

    private static void ComposeClosing(ColumnDescriptor col, OrderClosingPdfData data, bool isInspectionOnly)
    {
        var wo = data.WorkOrder;

        SectionTitle(col, "5. CIERRE DE LA ORDEN", data.Internal);

        // Puertas adentro no se resume: va el historial completo de estados con sus notas,
        // que es lo que sirve para reconstruir qué pasó cuando alguien reclama.
        if (data.Internal)
        {
            ComposeFullTimeline(col, wo);
            ComposeClosingFooter(col, wo, isInternal: true);
            return;
        }

        // Hitos reales de la visita. Los que no ocurrieron no se dibujan: una orden que se
        // cerró sin presupuesto no tuvo envío ni aprobación, y un renglón vacío solo genera
        // la duda de si falta algo.
        //
        // "Inspección terminada" sale de la transición a Diagnosing, salvo en una orden de
        // solo inspección, donde cerrar la inspección completa la orden y las dos fechas
        // son la misma.
        var entrada       = wo.CreatedAt;
        var inspeccionFin = isInspectionOnly
            ? FirstChangeTo(wo, WorkOrderStatus.Completed)
            : FirstChangeTo(wo, WorkOrderStatus.Diagnosing);
        var presupuesto   = FirstChangeTo(wo, WorkOrderStatus.AwaitingApproval);
        var aprobacion    = FirstChangeTo(wo, WorkOrderStatus.Approved);
        var enTaller      = FirstChangeTo(wo, WorkOrderStatus.InProgress);
        var finalizada    = FirstChangeTo(wo, WorkOrderStatus.Completed);
        var entregada     = FirstChangeTo(wo, WorkOrderStatus.Delivered);

        col.Item().Column(c =>
        {
            TimelineRow(c, "Ingreso del vehículo", entrada);

            if (inspeccionFin is not null)
                TimelineRow(c, "Inspección terminada", inspeccionFin);

            if (!isInspectionOnly)
            {
                if (presupuesto is not null)
                    TimelineRow(c, "Presupuesto enviado", presupuesto);
                if (aprobacion is not null)
                    TimelineRow(c, "Aprobación del cliente", aprobacion);
                // El auto puede volver días después de aprobar: este es el momento en que
                // entró al taller a trabajarse, y no coincide con el ingreso inicial.
                if (enTaller is not null)
                    TimelineRow(c, "Entrada al taller — inicio del trabajo", enTaller);
            }

            // En solo inspección el cierre YA se mostró como "Inspección terminada".
            if (!isInspectionOnly)
                TimelineRow(c, "Trabajo finalizado", finalizada);

            if (entregada is not null)
                TimelineRow(c, "Entrega al cliente", entregada);
        });

        ComposeClosingFooter(col, wo);
    }

    /// <summary>
    /// Historial completo de estados con notas — solo interna. Es el registro que permite
    /// reconstruir la orden cuando alguien pregunta "¿por qué se volvió a cotizar?".
    /// </summary>
    private static void ComposeFullTimeline(ColumnDescriptor col, WorkOrderDetailDto wo)
    {
        col.Item().Text("Historial completo de la orden").Bold().FontSize(8).FontColor(Muted);

        col.Item().PaddingTop(4).Column(c =>
        {
            c.Item().PaddingBottom(4).Row(row =>
            {
                row.RelativeItem().Text("Apertura de la orden").FontSize(10);
                row.ConstantItem(150).AlignRight()
                    .Text($"{wo.CreatedAt:dd/MM/yyyy HH:mm} hs").FontSize(10).Bold();
            });

            foreach (var change in wo.Timeline.OrderBy(t => t.ChangedAt))
            {
                c.Item().PaddingTop(4).PreventPageBreak().Column(inner =>
                {
                    inner.Item().Row(row =>
                    {
                        row.RelativeItem()
                            .Text(change.FromStatus is { } from
                                ? $"{StatusLabel(from)} → {StatusLabel(change.ToStatus)}"
                                : StatusLabel(change.ToStatus))
                            .FontSize(10);
                        row.ConstantItem(150).AlignRight()
                            .Text($"{change.ChangedAt:dd/MM/yyyy HH:mm} hs").FontSize(10).Bold();
                    });

                    if (!string.IsNullOrWhiteSpace(change.Note))
                        inner.Item().PaddingLeft(10)
                            .Text(change.Note).FontSize(8).Italic().FontColor(Muted);
                });
            }
        });
    }

    /// <summary>
    /// Cierre del documento: la nota del taller y, solo en la versión del cliente, el
    /// saludo. En la interna el saludo sobra — nadie se lo agradece a sí mismo.
    /// </summary>
    private static void ComposeClosingFooter(
        ColumnDescriptor col, WorkOrderDetailDto wo, bool isInternal = false)
    {
        if (!string.IsNullOrWhiteSpace(wo.TechnicianNote))
        {
            // Título y texto juntos: el título solo al pie de una página no dice nada.
            col.Item().PaddingTop(10).PreventPageBreak().Column(c =>
            {
                c.Item().Text("Observaciones del taller").Bold().FontSize(8).FontColor(Muted);
                c.Item().PaddingTop(2).Text(wo.TechnicianNote).FontSize(10).FontColor("#444444");
            });
        }

        if (isInternal) return;

        col.Item().PaddingTop(14).PreventPageBreak().Background(SoftBg).Padding(10).Column(c =>
        {
            c.Item().Text("Gracias por confiar en GB Service.").Bold().FontSize(10).FontColor(Ink);
            c.Item().PaddingTop(3)
                .Text($"Ante cualquier consulta sobre esta visita, mencioná el número de orden {wo.Number}.")
                .FontSize(9).FontColor("#444444");
        });
    }

    private static DateTime? FirstChangeTo(WorkOrderDetailDto wo, WorkOrderStatus status)
        => wo.Timeline
            .Where(t => t.ToStatus == status)
            .OrderBy(t => t.ChangedAt)
            .Select(t => (DateTime?)t.ChangedAt)
            .FirstOrDefault();

    private static void TimelineRow(ColumnDescriptor col, string label, DateTime? moment)
    {
        col.Item().PaddingVertical(2).Row(row =>
        {
            row.RelativeItem().Text(label).FontSize(10);
            row.ConstantItem(150).AlignRight()
                .Text(moment is { } m ? $"{m:dd/MM/yyyy HH:mm} hs" : "—")
                .FontSize(10).Bold();
        });
    }

    // ─── Helpers de maquetado ────────────────────────────────────────────────

    private static void SectionTitle(ColumnDescriptor col, string title, bool isInternal = false)
    {
        col.Item().PaddingTop(18).Text(title).Bold().FontSize(11).FontColor(Ink);
        col.Item().PaddingTop(3).PaddingBottom(8)
            .LineHorizontal(0.8f).LineColor(isInternal ? InternalAccent : Accent);
    }

    /// <summary>Par etiqueta/valor. Si el valor está vacío no se dibuja: un renglón con "—"
    /// por cada dato que el taller no carga ensucia el bloque sin aportar nada.</summary>
    private static void Field(ColumnDescriptor col, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        col.Item().PaddingTop(3).Row(row =>
        {
            row.ConstantItem(95).Text(label).FontSize(9).FontColor(Muted);
            row.RelativeItem().Text(value).FontSize(9).Bold();
        });
    }

    private static IContainer HeaderCell(IContainer c) => c.Background(Ink).Padding(6);
}
