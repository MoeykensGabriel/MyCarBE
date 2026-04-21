using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Common.Interfaces;
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
            IsAuthenticated = _currentUser.IsAuthenticated
        });
    }
}
