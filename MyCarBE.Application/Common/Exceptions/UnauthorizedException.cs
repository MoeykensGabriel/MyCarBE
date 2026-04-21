namespace MyCarBE.Application.Common.Exceptions;

/// <summary>
/// Se lanza cuando el usuario no está autenticado.
/// HTTP 401 Unauthorized.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Authentication is required to access this resource.") { }

    public UnauthorizedException(string message)
        : base(message) { }
}
