using MyCarBE.Application.Features.Auth.DTOs;

namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Abstracción de las operaciones de Identity (login, creación de usuarios).
/// Implementado en la capa Data usando UserManager y RoleManager de ASP.NET Identity.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Valida credenciales y retorna el token JWT si son correctas.
    /// Retorna null si el email no existe, la contraseña es incorrecta o el usuario está inactivo.
    /// </summary>
    Task<AuthResponseDto?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un ApplicationUser con el rol indicado y una contraseña temporal generada.
    /// Retorna el UserId y la contraseña temporal para que el Admin se la entregue al cliente.
    /// </summary>
    Task<(Guid UserId, string TempPassword)> CreateUserAsync(
        string email,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken = default);
}
