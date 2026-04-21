using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.Auth.Commands.Login;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
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
}
