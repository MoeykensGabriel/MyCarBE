using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Features.Auth.DTOs;

namespace MyCarBE.Application.Features.Auth.Commands.RefreshSession;

public class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, AuthResponseDto>
{
    private readonly IIdentityService    _identityService;
    private readonly ICurrentUserService _currentUser;

    public RefreshSessionCommandHandler(
        IIdentityService    identityService,
        ICurrentUserService currentUser)
    {
        _identityService = identityService;
        _currentUser     = currentUser;
    }

    public Task<AuthResponseDto> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        // IssueSessionAsync ya valida que el usuario siga activo.
        return _identityService.IssueSessionAsync(_currentUser.UserId, cancellationToken);
    }
}
