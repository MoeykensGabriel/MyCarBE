using System.Globalization;

namespace MyCarBE.Application.Common.Formatting;

/// <summary>
/// Formato de moneda argentino, SIEMPRE con cultura explícita: "$ 10.000,00"
/// (punto para miles, coma para decimales). Nunca usar ":N0"/":C" a secas en
/// montos que ve el cliente — dependen de la cultura del servidor y en el
/// contenedor de producción (Linux, cultura invariante) salen "10,000".
/// </summary>
public static class MoneyFormat
{
    private static readonly CultureInfo EsAr = CultureInfo.GetCultureInfo("es-AR");

    /// <summary>"$ 10.000,00" — para PDFs, emails y cualquier salida del BE.</summary>
    public static string ArCurrency(decimal amount) => $"$ {amount.ToString("N2", EsAr)}";

    /// <summary>
    /// "10.000" — número entero con separador de miles es-AR (kilometrajes, cantidades).
    /// Mismo motivo que ArCurrency: ":N0" a secas depende de la cultura del servidor.
    /// </summary>
    public static string ArNumber(long value) => value.ToString("N0", EsAr);
}
