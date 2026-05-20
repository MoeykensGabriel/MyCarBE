using MediatR;
using MyCarBE.Application.Common.Interfaces;

namespace MyCarBE.Application.Features.Auth.Commands.AdminResetPassword;

public class AdminResetPasswordCommandHandler
    : IRequestHandler<AdminResetPasswordCommand, AdminResetPasswordResponse>
{
    private readonly IIdentityService _identityService;

    public AdminResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AdminResetPasswordResponse> Handle(
        AdminResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tempPassword = await _identityService.ResetPasswordAsync(request.UserId, cancellationToken);
        return new AdminResetPasswordResponse(tempPassword);
    }
}
