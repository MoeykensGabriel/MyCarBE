using MediatR;
using MyCarBE.Application.Features.Auth.DTOs;

namespace MyCarBE.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
