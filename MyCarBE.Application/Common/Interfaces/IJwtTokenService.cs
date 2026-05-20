namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Genera tokens JWT firmados con los claims del usuario.
/// Implementado en la capa Data usando System.IdentityModel.Tokens.Jwt.
/// </summary>
public interface IJwtTokenService
{
    /// <param name="customerId">Se incluye cuando el usuario tiene rol Customer. Null para Admin/Mechanic.</param>
    /// <param name="fleetId">Se incluye cuando el Customer pertenece a una flota. Null si es particular.</param>
    /// <param name="mechanicId">Se incluye cuando el usuario tiene rol Mechanic. Null para Admin/Customer.</param>
    string GenerateToken(Guid userId, string email, string role, string fullName,
        Guid? customerId = null, Guid? fleetId = null, Guid? mechanicId = null);
}
