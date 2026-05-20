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
    bool IsMechanic { get; }
    bool IsAuthenticated { get; }

    /// <summary>
    /// Id del Customer vinculado al usuario. Null si el usuario es Admin o Mechanic.
    /// </summary>
    Guid? CustomerId { get; }

    /// <summary>
    /// Id de la flota a la que pertenece el Customer. Null si es un particular, un Admin o un Mechanic.
    /// </summary>
    Guid? FleetId { get; }

    /// <summary>
    /// Id del Mechanic vinculado al usuario. Solo presente cuando el rol es Mechanic.
    /// </summary>
    Guid? MechanicId { get; }
}
