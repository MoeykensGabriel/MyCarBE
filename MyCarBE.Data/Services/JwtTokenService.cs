using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyCarBE.Application.Common.Interfaces;

namespace MyCarBE.Data.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string email, string role, string fullName,
        Guid? customerId = null, Guid? fleetId = null, Guid? mechanicId = null)
    {
        var jwtSettings  = _configuration.GetSection("JwtSettings");
        var secretKey    = jwtSettings["SecretKey"]!;
        var issuer       = jwtSettings["Issuer"]!;
        var audience     = jwtSettings["Audience"]!;
        var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"]!);

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name,  fullName),
            new Claim(ClaimTypes.Role,               role),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        // Claims opcionales — solo presentes en tokens de Customer
        if (customerId.HasValue)
            claims.Add(new Claim("customerId", customerId.Value.ToString()));
        if (fleetId.HasValue)
            claims.Add(new Claim("fleetId", fleetId.Value.ToString()));
        // Claim para tokens de Mechanic
        if (mechanicId.HasValue)
            claims.Add(new Claim("mechanicId", mechanicId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
