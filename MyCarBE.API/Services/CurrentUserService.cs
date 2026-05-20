using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MyCarBE.Application.Common.Interfaces;

namespace MyCarBE.API.Services;

/// <summary>
/// Lee los claims del JWT del request actual y los expone a los Handlers via ICurrentUserService.
/// Vive en la API porque depende de HttpContext — Application no puede tocarlo.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var value = User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email =>
        User?.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;

    public string Role =>
        User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public bool IsAdmin =>
        User?.IsInRole("Admin") ?? false;

    public bool IsMechanic =>
        User?.IsInRole("Mechanic") ?? false;

    public bool IsReceptionist =>
        User?.IsInRole("Receptionist") ?? false;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public Guid? CustomerId
    {
        get
        {
            var value = User?.FindFirstValue("customerId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? FleetId
    {
        get
        {
            var value = User?.FindFirstValue("fleetId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? MechanicId
    {
        get
        {
            var value = User?.FindFirstValue("mechanicId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
