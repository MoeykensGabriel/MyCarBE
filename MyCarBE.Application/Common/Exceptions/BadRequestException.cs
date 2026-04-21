namespace MyCarBE.Application.Common.Exceptions;

/// <summary>
/// Se lanza cuando la lógica de negocio rechaza la operación por datos inválidos
/// que no son capturados por FluentValidation (ej: transición de estado inválida).
/// HTTP 400 Bad Request.
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message)
        : base(message) { }
}
