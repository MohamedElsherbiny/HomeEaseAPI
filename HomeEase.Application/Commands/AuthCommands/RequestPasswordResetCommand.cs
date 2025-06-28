using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HomeEase.Application.Commands.AuthCommands;

public class RequestPasswordResetCommand(PasswordResetRequestDto dto) : IRequest<bool>
{
    public string Email { get; set; } = dto.Email;
}

public class RequestPasswordResetCommandHandler(UserManager<User> _userManager, IEmailService _emailService, ICurrentUserService _currentUserService) : IRequestHandler<RequestPasswordResetCommand, bool>
{
    public async Task<bool> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return true;

        var otp = await _userManager.GenerateUserTokenAsync(user, "OtpProvider", "ResetPassword");
        await _emailService.SendPasswordResetEmailAsync(user.Email!, otp, _currentUserService.Language);

        return true;
    }
}