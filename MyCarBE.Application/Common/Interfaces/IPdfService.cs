using MyCarBE.Application.Common.Models;

namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Generación de PDFs transaccionales.
/// Implementado en API con QuestPDF; intercambiable por otro proveedor sin tocar Application.
/// </summary>
public interface IPdfService
{
    byte[] GenerateQuotePdf(QuotePdfData data);
}
