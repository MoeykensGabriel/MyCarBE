using MediatR;
using MyCarBE.Application.Common.Interfaces;

namespace MyCarBE.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IIdentityService    _identityService;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(
        IIdentityService    identityService,
        ICurrentUserService currentUser)
    {
        _identityService = identityService;
        _currentUser     = currentUser;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        await _identityService.ChangePasswordAsync(
            _currentUser.UserId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);
    }
}
