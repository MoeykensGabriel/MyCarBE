using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MyCarBE.API.Services;

public class QuotePdfService : IPdfService
{
    public byte[] GenerateQuotePdf(QuotePdfData data)
    {
        var wo = data.WorkOrder;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                // ── HEADER ──────────────────────────────────────────────────
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("MyCarApp").Bold().FontSize(22).FontColor("#1a1a2e");
                            c.Item().Text("Taller de Servicios Automotores").FontSize(11).FontColor("#555");
                        });

                        row.ConstantItem(150).Column(c =>
                        {
                            c.Item().AlignRight().Text("PRESUPUESTO").Bold().FontSize(16).FontColor("#e63946");
                            c.Item().AlignRight().Text($"Nº {wo.Id.ToString()[..8].ToUpper()}").FontSize(9).FontColor("#777");
                            c.Item().AlignRight().Text($"Fecha: {wo.CreatedAt:dd/MM/yyyy}").FontSize(9);
                        });
                    });

                    col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor("#e63946");
                });

                // ── CONTENT ─────────────────────────────────────────────────
                page.Content().PaddingTop(16).Column(col =>
                {
                    // Vehículo
                    col.Item().Background("#f8f9fa").Padding(10).Column(c =>
                    {
                        c.Item().Text("DATOS DEL VEHÍCULO").Bold().FontSize(9).FontColor("#777");
                        c.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text($"{data.VehicleBrand} {data.VehicleModel} {data.VehicleYear}").Bold().FontSize(12);
                            r.ConstantItem(120).AlignRight().Text($"Patente: {data.LicensePlate}").Bold();
                        });
                    });

                    col.Item().PaddingTop(10);

                    // Cliente / Destinatario
                    col.Item().Column(c =>
                    {
                        c.Item().Text("PRESUPUESTO PARA").Bold().FontSize(9).FontColor("#777");
                        c.Item().PaddingTop(2).Text(data.RecipientName).Bold().FontSize(11);
                        c.Item().Text(data.RecipientEmail).FontColor("#555");
                    });

                    col.Item().PaddingTop(14);

                    // Nota del cliente
                    if (!string.IsNullOrWhiteSpace(wo.CustomerNote))
                    {
                        col.Item().Column(c =>
                        {
                            c.Item().Text("DESCRIPCIÓN DEL PROBLEMA").Bold().FontSize(9).FontColor("#777");
                            c.Item().PaddingTop(4).Text(wo.CustomerNote).Italic().FontColor("#444");
                        });
                        col.Item().PaddingTop(14);
                    }

                    // Tabla de servicios
                    col.Item().Text("SERVICIOS").Bold().FontSize(9).FontColor("#777");
                    col.Item().PaddingTop(6).Table(table =>
                    {
                        // Sin precio unitario (pedido del taller: el cliente ve solo el subtotal
                        // por línea) y sin la descripción del servicio (ahí viajan las novedades
                        // de la inspección y el "reportado por {mecánico}" — material interno).
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(35);   // Cant.
                            cols.RelativeColumn();      // Servicio
                            cols.ConstantColumn(90);   // Subtotal
                        });

                        // Encabezado
                        static IContainer HeaderCell(IContainer c) =>
                            c.Background("#1a1a2e").Padding(6);

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).AlignCenter()
                                .Text("Cant.").FontColor(Colors.White).Bold().FontSize(9);
                            h.Cell().Element(HeaderCell)
                                .Text("Servicio").FontColor(Colors.White).Bold().FontSize(9);
                            h.Cell().Element(HeaderCell).AlignRight()
                                .Text("Subtotal").FontColor(Colors.White).Bold().FontSize(9);
                        });

                        // Filas
                        var isOdd = false;
                        foreach (var service in wo.Services)
                        {
                            var bg = isOdd ? "#f8f9fa" : "#ffffff";
                            isOdd = !isOdd;

                            IContainer Cell(IContainer c) => c.Background(bg).Padding(6);

                            table.Cell().Element(Cell).AlignCenter().Text(service.Quantity.ToString());
                            table.Cell().Element(Cell).Text(service.NameSnapshot).Bold();
                            table.Cell().Element(Cell).AlignRight()
                                .Text($"$ {service.Subtotal:N0}").Bold();
                        }
                    });

                    // Tabla de repuestos — resumida: el cliente ve nombre, cantidad y precio,
                    // pero NO el código de proveedor del taller.
                    if (wo.Parts.Any())
                    {
                        col.Item().PaddingTop(14);
                        col.Item().Text("REPUESTOS").Bold().FontSize(9).FontColor("#777");
                        col.Item().PaddingTop(6).Table(partsTable =>
                        {
                            // Sin precio unitario — misma regla que los servicios.
                            partsTable.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(35);   // Cant.
                                cols.RelativeColumn();      // Repuesto
                                cols.ConstantColumn(90);   // Subtotal
                            });

                            static IContainer PartHeaderCell(IContainer c) =>
                                c.Background("#1a1a2e").Padding(6);

                            partsTable.Header(h =>
                            {
                                h.Cell().Element(PartHeaderCell).AlignCenter()
                                    .Text("Cant.").FontColor(Colors.White).Bold().FontSize(9);
                                h.Cell().Element(PartHeaderCell)
                                    .Text("Repuesto").FontColor(Colors.White).Bold().FontSize(9);
                                h.Cell().Element(PartHeaderCell).AlignRight()
                                    .Text("Subtotal").FontColor(Colors.White).Bold().FontSize(9);
                            });

                            var isOddPart = false;
                            foreach (var part in wo.Parts)
                            {
                                var bg = isOddPart ? "#f8f9fa" : "#ffffff";
                                isOddPart = !isOddPart;

                                IContainer PartCell(IContainer c) => c.Background(bg).Padding(6);

                                partsTable.Cell().Element(PartCell).AlignCenter().Text(part.Quantity.ToString());
                                partsTable.Cell().Element(PartCell).Text(part.Name).Bold();
                                partsTable.Cell().Element(PartCell).AlignRight()
                                    .Text($"$ {part.Subtotal:N0}").Bold();
                            }
                        });
                    }

                    // Total
                    col.Item().PaddingTop(8).AlignRight().Row(r =>
                    {
                        r.ConstantItem(180).Background("#1a1a2e").Padding(10).Row(inner =>
                        {
                            inner.RelativeItem().Text("TOTAL").FontColor(Colors.White).Bold().FontSize(13);
                            inner.RelativeItem().AlignRight()
                                .Text($"$ {wo.TotalAmount:N0}").FontColor(Colors.White).Bold().FontSize(13);
                        });
                    });

                    col.Item().PaddingTop(20);

                    // Aviso de aprobación + validez (14 días desde el envío)
                    col.Item().Background("#fff3cd").Padding(10).Column(c =>
                    {
                        c.Item().Text("⚠ Este presupuesto requiere su aprobación para continuar con el trabajo.")
                            .Bold().FontSize(9).FontColor("#856404");
                        c.Item().PaddingTop(4)
                            .Text("Al aprobar, autoriza la realización de los servicios y repuestos detallados arriba.")
                            .FontSize(9).FontColor("#856404");
                        c.Item().PaddingTop(4)
                            .Text(wo.QuoteExpiresAt is { } quoteExpiresAt
                                ? $"Presupuesto válido hasta el {quoteExpiresAt:dd/MM/yyyy}. Pasada esa fecha, solicite uno actualizado."
                                : "Validez del presupuesto: 14 días desde su envío.")
                            .Bold().FontSize(9).FontColor("#856404");
                    });
                });

                // ── FOOTER ──────────────────────────────────────────────────
                page.Footer().PaddingTop(8).Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor("#ccc");
                    col.Item().PaddingTop(4).AlignCenter()
                        .Text("MyCarApp — Sistema de Gestión de Taller")
                        .FontSize(8).FontColor("#aaa");
                });
            });
        }).GeneratePdf();
    }
}
