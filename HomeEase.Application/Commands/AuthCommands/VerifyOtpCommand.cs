using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace HomeEase.Application.Commands.AuthCommands;

public class VerifyOtpCommand : IRequest<bool>
{
    public string Email { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
}

public class VerifyOtpCommandHandler(UserManager<User> _userManager) : IRequestHandler<VerifyOtpCommand, bool>
{
    public async Task<bool> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return false;

        var isValid = await _userManager.VerifyUserTokenAsync(user, "OtpProvider", "ResetPassword", request.OtpCode);
        return isValid;
    }
}