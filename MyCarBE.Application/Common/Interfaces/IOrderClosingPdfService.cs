using MyCarBE.Application.Common.Models;

namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Generación del INFORME DE CIERRE de una orden.
///
/// Va en su propia interfaz y no como un método más de <see cref="IPdfService"/> a propósito:
/// aquella la implementa QuotePdfService, que es el documento del presupuesto y ya es largo.
/// Son dos documentos distintos, con reglas distintas sobre qué se muestra, y conviene que
/// puedan evolucionar por separado.
/// </summary>
public interface IOrderClosingPdfService
{
    byte[] GenerateClosingReport(OrderClosingPdfData data);
}
