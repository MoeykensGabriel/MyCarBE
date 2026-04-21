namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Provee información del usuario autenticado en el request actual.
/// Implementado en la capa API usando IHttpContextAccessor.
/// Los Handlers lo inyectan para saber quién ejecuta la operación.
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
}
