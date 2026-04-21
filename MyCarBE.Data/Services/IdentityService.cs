using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Features.Auth.DTOs;
using MyCarBE.Data.Identity;

namespace MyCarBE.Data.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly IJwtTokenService              _jwtTokenService;
    private readonly IConfiguration                _configuration;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _userManager     = userManager;
        _jwtTokenService = jwtTokenService;
        _configuration   = configuration;
    }

    public async Task<AuthResponseDto?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
            return null;

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            return null;

        var roles    = await _userManager.GetRolesAsync(user);
        var role     = roles.FirstOrDefault() ?? string.Empty;
        var fullName = user.UserName ?? email;

        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"]!);
        var token = _jwtTokenService.GenerateToken(user.Id, email, role, fullName);

        return new AuthResponseDto
        {
            Token     = token,
            Role      = role,
            Email     = email,
            FullName  = fullName,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }

    public async Task<(Guid UserId, string TempPassword)> CreateUserAsync(
        string email,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken = default)
    {
        var tempPassword = GenerateTempPassword();

        var user = new ApplicationUser
        {
            Id        = Guid.NewGuid(),
            UserName  = email,
            Email     = email,
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, tempPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, role);

        return (user.Id, tempPassword);
    }

    // -------------------------------------------------------------------------
    // Genera una contraseña temporal que cumple las reglas de Identity:
    // mínimo 8 chars, 1 mayúscula, 1 dígito.
    // Formato: "Mc" + 4 letras random + 4 dígitos → ej: "McXkpq7341"
    // -------------------------------------------------------------------------
    private static string GenerateTempPassword()
    {
        const string letters = "abcdefghijkmnopqrstuvwxyz";
        const string digits  = "23456789";

        var rng  = Random.Shared;
        var body = new string(Enumerable.Range(0, 4).Select(_ => letters[rng.Next(letters.Length)]).ToArray());
        var nums = new string(Enumerable.Range(0, 4).Select(_ => digits[rng.Next(digits.Length)]).ToArray());

        return $"Mc{body}{nums}"; // "Mc" ya aporta mayúscula + cumple prefijo reconocible
    }
}
