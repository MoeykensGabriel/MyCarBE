using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Auth.DTOs;
using MyCarBE.Data.Identity;

namespace MyCarBE.Data.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly IJwtTokenService              _jwtTokenService;
    private readonly ICustomerRepository           _customerRepository;
    private readonly IMechanicRepository           _mechanicRepository;
    private readonly IConfiguration                _configuration;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ICustomerRepository customerRepository,
        IMechanicRepository mechanicRepository,
        IConfiguration configuration)
    {
        _userManager        = userManager;
        _jwtTokenService    = jwtTokenService;
        _customerRepository = customerRepository;
        _mechanicRepository = mechanicRepository;
        _configuration      = configuration;
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

        // Para usuarios con rol Customer/Mechanic, incluimos los Ids de dominio en el JWT
        // para que los handlers puedan aplicar ownership sin tocar la base de datos
        Guid? customerId = null;
        Guid? fleetId    = null;
        Guid? mechanicId = null;

        if (role == "Customer")
        {
            var customer = await _customerRepository.GetByApplicationUserIdAsync(user.Id, cancellationToken);
            if (customer is not null)
            {
                customerId = customer.Id;
                fleetId    = customer.FleetId;
                fullName   = $"{customer.FirstName} {customer.LastName}".Trim();
            }
        }
        else if (role == "Mechanic")
        {
            var mechanic = await _mechanicRepository.GetByApplicationUserIdAsync(user.Id, cancellationToken);
            if (mechanic is not null)
            {
                if (!mechanic.IsActive)
                    return null; // mecánico desactivado no puede ingresar
                mechanicId = mechanic.Id;
                fullName   = $"{mechanic.FirstName} {mechanic.LastName}".Trim();
            }
        }

        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"]!);
        var token = _jwtTokenService.GenerateToken(user.Id, email, role, fullName, customerId, fleetId, mechanicId);

        return new AuthResponseDto
        {
            Token      = token,
            Role       = role,
            Email      = email,
            FullName   = fullName,
            ExpiresAt  = DateTime.UtcNow.AddMinutes(expirationMinutes),
            UserId     = user.Id,
            CustomerId = customerId,
            FleetId    = fleetId,
            MechanicId = mechanicId,
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

    public async Task ChangePasswordAsync(
        Guid   userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("ApplicationUser", userId);

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }
    }

    public async Task<string> ResetPasswordAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("ApplicationUser", userId);

        var newPassword = GenerateTempPassword();

        // Usamos Remove + Add en vez de GeneratePasswordResetToken porque
        // `AddIdentityCore` no registra TokenProviders por defecto (haría falta
        // .AddDefaultTokenProviders() en el DI). Remove+Add es atómico para
        // este caso y no depende de configuración extra.
        if (await _userManager.HasPasswordAsync(user))
        {
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }
        }

        var addResult = await _userManager.AddPasswordAsync(user, newPassword);
        if (!addResult.Succeeded)
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }

        return newPassword;
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
