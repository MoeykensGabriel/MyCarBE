namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Genera tokens JWT firmados con los claims del usuario.
/// Implementado en la capa Data usando System.IdentityModel.Tokens.Jwt.
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, string role, string fullName);
}
