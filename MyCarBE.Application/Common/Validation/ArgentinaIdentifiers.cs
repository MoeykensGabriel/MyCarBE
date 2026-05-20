using System.Text.RegularExpressions;

namespace MyCarBE.Application.Common.Validation;

/// <summary>
/// Helpers de validación y normalización para identificadores argentinos:
/// DNI, CUIT/CUIL y números de teléfono.
///
/// Diseño:
/// - Tolerantes a separadores comunes (puntos en DNI, guiones en CUIT, espacios y paréntesis en teléfonos).
/// - Las funciones <c>IsValid*</c> validan el formato (y para CUIT/CUIL también el dígito verificador).
/// - Las funciones <c>Normalize*</c> devuelven una representación canónica para almacenar.
/// </summary>
public static class ArgentinaIdentifiers
{
    private static readonly Regex NonDigit = new(@"\D", RegexOptions.Compiled);

    // ─── DNI ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// DNI argentino: 7 u 8 dígitos numéricos.
    /// Acepta entrada con puntos: "12.345.678" o "12345678".
    /// </summary>
    public static bool IsValidDni(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        var digits = NonDigit.Replace(input, "");
        return digits.Length is 7 or 8;
    }

    /// <summary>Devuelve el DNI sin separadores. Asume input válido.</summary>
    public static string NormalizeDni(string input) => NonDigit.Replace(input, "");

    // ─── CUIT / CUIL ──────────────────────────────────────────────────────────

    /// <summary>
    /// CUIT/CUIL argentino: solo verifica que tenga 11 dígitos (sin checksum).
    /// Decisión de producto: priorizamos no bloquear al admin con falsos negativos
    /// sobre atrapar todos los typos. El front hace la misma validación.
    /// Acepta entrada con o sin guiones: "30-12345678-9" o "30123456789".
    /// </summary>
    public static bool IsValidCuitOrCuil(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        var digits = NonDigit.Replace(input, "");
        return digits.Length == 11;
    }

    /// <summary>
    /// Devuelve el CUIT con guiones canónicos: "XX-XXXXXXXX-X".
    /// Asume input válido (sino devuelve la entrada sin tocar).
    /// </summary>
    public static string NormalizeCuit(string input)
    {
        var digits = NonDigit.Replace(input, "");
        if (digits.Length != 11) return input;
        return $"{digits[..2]}-{digits.Substring(2, 8)}-{digits[10]}";
    }

    // ─── Teléfono ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Validación tolerante de teléfono: entre 8 y 14 dígitos después de
    /// limpiar separadores comunes (espacios, guiones, paréntesis, puntos)
    /// y un eventual prefijo "+".
    /// No es estricto sobre el prefijo +54 para no bloquear casos legítimos.
    /// </summary>
    public static bool IsValidPhone(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        // Permitir un único "+" inicial; lo eliminamos para contar dígitos puros
        var trimmed = input.Trim();
        if (trimmed.StartsWith("+")) trimmed = trimmed[1..];

        var digits = NonDigit.Replace(trimmed, "");
        return digits.Length is >= 8 and <= 14;
    }

    /// <summary>
    /// Devuelve el teléfono sin separadores, conservando el "+" inicial si
    /// estaba presente. Ejemplo: "+54 9 11 1234-5678" → "+5491112345678".
    /// </summary>
    public static string NormalizePhone(string input)
    {
        var trimmed = input.Trim();
        var hasPlus = trimmed.StartsWith("+");
        var digits  = NonDigit.Replace(trimmed, "");
        return hasPlus ? "+" + digits : digits;
    }

    // ─── Pasaporte ────────────────────────────────────────────────────────────

    private static readonly Regex PassportRegex = new(@"^[A-Z0-9]{5,15}$", RegexOptions.Compiled);

    /// <summary>
    /// Pasaporte: alfanumérico, entre 5 y 15 caracteres. Case-insensitive
    /// (lo normalizamos a mayúsculas para validar).
    /// </summary>
    public static bool IsValidPassport(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return PassportRegex.IsMatch(input.Trim().ToUpperInvariant());
    }

    /// <summary>Pasaporte normalizado: trim + mayúsculas.</summary>
    public static string NormalizePassport(string input) => input.Trim().ToUpperInvariant();
}
