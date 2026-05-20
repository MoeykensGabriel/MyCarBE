namespace MyCarBE.Application.Common.Exceptions;

/// <summary>
/// Se lanza cuando el usuario está autenticado pero no tiene permisos para la operación.
/// HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("You do not have permission to perform this action.") { }

    public ForbiddenException(string message)
        : base(message) { }
}
