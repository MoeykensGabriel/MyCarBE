using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Auth.DTOs;
using MyCarBE.Data.Identity;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly IJwtTokenService              _jwtTokenService;
    private readonly ICustomerRepository           _customerRepository;
    private readonly IMechanicRepository           _mechanicRepository;
    private readonly IReceptionistRepository       _receptionistRepository;
    private readonly IUnitOfWork                   _unitOfWork;
    private readonly IConfiguration                _configuration;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ICustomerRepository customerRepository,
        IMechanicRepository mechanicRepository,
        IReceptionistRepository receptionistRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _userManager            = userManager;
        _jwtTokenService        = jwtTokenService;
        _customerRepository     = customerRepository;
        _mechanicRepository     = mechanicRepository;
        _receptionistRepository = receptionistRepository;
        _unitOfWork             = unitOfWork;
        _configuration          = configuration;
    }

    public async Task<AuthResponseDto?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
            return null;

        // Anti fuerza-bruta: si la cuenta está bloqueada por demasiados intentos
        // fallidos, rechazamos sin siquiera verificar la contraseña.
        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedException(
                "Demasiados intentos fallidos. La cuenta quedó bloqueada temporalmente. Probá de nuevo en unos minutos.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            // Incrementa el contador de fallos; al llegar al máximo, Identity bloquea
            // la cuenta por el DefaultLockoutTimeSpan configurado en DataLayerExtensions.
            await _userManager.AccessFailedAsync(user);
            return null;
        }

        // Login OK → reseteamos el contador de fallos.
        await _userManager.ResetAccessFailedCountAsync(user);

        return await BuildAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> IssueSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("ApplicationUser", userId);

        // El usuario ya está autenticado (viene de un endpoint [Authorize]), así que no
        // revalidamos contraseña ni lockout. Sí respetamos IsActive por si lo desactivaron
        // con la sesión abierta.
        if (!user.IsActive)
            throw new UnauthorizedException("El usuario está desactivado.");

        return await BuildAuthResponseAsync(user, cancellationToken)
            ?? throw new UnauthorizedException("No se pudo emitir la sesión para este usuario.");
    }

    /// <summary>
    /// Arma el token + DTO de sesión resolviendo los Ids de dominio del usuario.
    /// Lo comparten LoginAsync e IssueSessionAsync para que ambos emitan exactamente el
    /// mismo formato de token. Devuelve null solo cuando el perfil de dominio veta el ingreso
    /// (mecánico o recepcionista desactivado).
    /// </summary>
    private async Task<AuthResponseDto?> BuildAuthResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var email    = user.Email ?? user.UserName ?? string.Empty;
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

            return BuildDto();
        }

        if (role == "Receptionist")
        {
            var receptionist = await _receptionistRepository.GetByApplicationUserIdAsync(user.Id, cancellationToken);
            if (receptionist is not null)
            {
                if (!receptionist.IsActive)
                    return null; // recepcionista desactivado no puede ingresar
                fullName = $"{receptionist.FirstName} {receptionist.LastName}".Trim();
            }
        }

        // El claim mechanicId no depende del rol sino de tener un perfil de ejecutante
        // vinculado: un Admin trabaja como un mecánico más. Por eso el lookup corre para
        // todo rol que no sea Customer.
        var mechanic = await _mechanicRepository.GetByApplicationUserIdAsync(user.Id, cancellationToken);

        // El admin es ejecutante desde el minuto cero, sin pasos previos — mismo criterio
        // que la inspección, que ya puede hacer él mismo sin habilitar nada. Si todavía no
        // tiene perfil se lo creamos acá: es la única forma de que pueda tomar trabajos en
        // un taller sin mecánicos cargados, que es el arranque normal de cualquier cliente.
        if (mechanic is null && role == "Admin")
            mechanic = await CreateExecutorProfileForAdminAsync(user, email, cancellationToken);

        if (mechanic is not null)
        {
            if (role == "Mechanic")
            {
                // Para un mecánico puro, el perfil ES su identidad: desactivado no entra.
                if (!mechanic.IsActive)
                    return null;

                mechanicId = mechanic.Id;
                fullName   = $"{mechanic.FirstName} {mechanic.LastName}".Trim();
            }
            else if (mechanic.IsActive)
            {
                // Admin/oficina con perfil de ejecutante: sumamos el claim pero NO pisamos
                // el fullName — su identidad principal sigue siendo la de su rol. Si el
                // perfil está desactivado simplemente no emitimos el claim; jamás le
                // bloqueamos el ingreso, porque necesita seguir administrando.
                mechanicId = mechanic.Id;
            }
        }

        return BuildDto();

        AuthResponseDto BuildDto()
        {
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
    }

    /// <summary>
    /// Crea el perfil de ejecutante del admin la primera vez que entra, para que pueda tomar
    /// y hacer trabajos sin ningún paso de habilitación previo.
    ///
    /// Devuelve null (sin romper el login) cuando el perfil no se puede crear: el email ya
    /// pertenece a otro mecánico, o una carrera entre dos logins simultáneos lo creó primero.
    /// Entrar al sistema nunca puede depender de esto — en el peor caso el admin queda sin el
    /// claim y lo resuelve el siguiente login.
    /// </summary>
    private async Task<Mechanic?> CreateExecutorProfileForAdminAsync(
        ApplicationUser user,
        string email,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
            return null;

        // El email del Mechanic es único. Si ya lo usa otro perfil no pisamos nada: el admin
        // puede vincularse a mano desde el panel de mecánicos.
        if (await _mechanicRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
            return null;

        var (firstName, lastName) = DeriveExecutorName(user.UserName, normalizedEmail);

        // Sin Id a mano: lo pone SaveChangesAsync sobre la entidad trackeada, así que
        // mechanic.Id ya viene resuelto cuando volvemos.
        var mechanic = new Mechanic
        {
            FirstName         = firstName,
            LastName          = lastName,
            Email             = normalizedEmail,
            Phone             = null,
            Specialty         = null,
            IsActive          = true,
            // Generalista siempre: el admin no tiene áreas asignadas y ya inspecciona
            // cualquier área hoy (por la rama "oficina" de CreateInspectionReport). Sin este
            // flag, en cuanto reciba el claim mechanicId caería en la validación por áreas
            // y perdería una capacidad que ya tenía.
            IsGeneralist      = true,
            ApplicationUserId = user.Id,
        };

        try
        {
            await _mechanicRepository.AddAsync(mechanic, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return mechanic;
        }
        catch (DbUpdateException)
        {
            // Dos logins en paralelo: el índice único de ApplicationUserId frenó al segundo.
            // Nos quedamos con el que ganó.
            return await _mechanicRepository.GetByApplicationUserIdAsync(user.Id, cancellationToken);
        }
    }

    /// <summary>
    /// Nombre visible del perfil recién creado. Los usuarios Admin no tienen nombre y apellido
    /// propios, así que lo derivamos del usuario ("juan.perez@taller.com" → "Juan Perez").
    /// Es solo una semilla: el admin lo renombra cuando quiera desde Empresa → Mecánicos.
    /// </summary>
    private static (string FirstName, string LastName) DeriveExecutorName(string? userName, string email)
    {
        // El UserName suele ser el mismo email; nos quedamos con la parte local igual.
        var source = string.IsNullOrWhiteSpace(userName) ? email : userName;
        var local  = source.Split('@')[0];

        var words = local
            .Split(new[] { '.', '_', '-', '+', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant())
            .ToArray();

        return words.Length switch
        {
            0 => ("Admin", "Taller"),
            1 => (Truncate(words[0]), "(admin)"),
            _ => (Truncate(words[0]), Truncate(string.Join(" ", words[1..]))),
        };

        // FirstName/LastName están limitados a 100 chars en la config de EF.
        static string Truncate(string value) => value.Length <= 100 ? value : value[..100];
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
