using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Features.Auth.Commands.AdminResetPassword;
using MyCarBE.Application.Features.Auth.Commands.ChangePassword;
using MyCarBE.Application.Features.Auth.Commands.Login;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender            _sender;
    private readonly ICurrentUserService _currentUser;

    public AuthController(ISender sender, ICurrentUserService currentUser)
    {
        _sender      = sender;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Login para Admin y Customer. Retorna un JWT con el rol en los claims.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Application.Features.Auth.DTOs.AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Devuelve los datos del usuario autenticado extraídos del JWT.
    /// Útil para verificar que el token es válido y el rol correcto.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId          = _currentUser.UserId,
            Email           = _currentUser.Email,
            Role            = _currentUser.Role,
            IsAdmin         = _currentUser.IsAdmin,
            IsAuthenticated = _currentUser.IsAuthenticated,
            CustomerId      = _currentUser.CustomerId,
            FleetId         = _currentUser.FleetId
        });
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado.
    /// Requiere la contraseña actual para confirmar la identidad.
    /// Disponible para Admin y Customer.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Reset administrativo de contraseña. Solo Admin.
    /// Genera una nueva contraseña temporal y la devuelve en la respuesta para
    /// que el Admin se la entregue al usuario por canal seguro.
    /// </summary>
    [HttpPost("users/{userId:guid}/reset-password")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AdminResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdminResetPassword(
        Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AdminResetPasswordCommand(userId), cancellationToken);
        return Ok(result);
    }
}
