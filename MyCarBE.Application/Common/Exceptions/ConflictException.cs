namespace MyCarBE.Application.Common.Exceptions;

/// <summary>
/// Se lanza cuando se intenta crear un recurso que ya existe (patente duplicada, DNI duplicado, etc.).
/// HTTP 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string entityName, string field, object value)
        : base($"'{entityName}' with {field} '{value}' already exists.") { }
}
