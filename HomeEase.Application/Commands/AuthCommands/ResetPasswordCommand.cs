using HomeEase.Application.DTOs;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace HomeEase.Application.Commands.AuthCommands;

public class ResetPasswordCommand(PasswordResetDto dto) : IRequest<bool>
{
    public string OtpCode { get; set; } = dto.OtpCode;
    public string Email { get; set; } = dto.Email;
    public string NewPassword { get; set; } = dto.NewPassword;
}

public class ResetPasswordCommandHandler(UserManager<User> _userManager) : IRequestHandler<ResetPasswordCommand, bool>
{
    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new ApplicationException("User not found.");
        }

        var isValid = await _userManager.VerifyUserTokenAsync(user, "OtpProvider", "ResetPassword", request.OtpCode);
        if (!isValid)
            throw new ApplicationException("Invalid or expired code");

        var result = await _userManager.ResetPasswordAsync(user, await _userManager.GeneratePasswordResetTokenAsync(user), request.NewPassword);
        if (!result.Succeeded)
            throw new ApplicationException($"Password reset failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        return true;
    }
}
