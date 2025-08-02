using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HomeEase.Application.Commands.AuthCommands;

public class RequestPasswordResetCommand(PasswordResetRequestDto dto) : IRequest<EntityResult>
{
    public string Email { get; set; } = dto.Email;
}

public class RequestPasswordResetCommandHandler(UserManager<User> _userManager, IEmailService _emailService, ICurrentUserService _currentUserService) : IRequestHandler<RequestPasswordResetCommand, EntityResult>
{
    public async Task<EntityResult> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return EntityResult.Success;
        }

        var otp = await _userManager.GenerateUserTokenAsync(user, "OtpProvider", "ResetPassword");
        await _emailService.SendPasswordResetEmailAsync(user.Email!, otp, _currentUserService.Language);

        return EntityResult.Success;
    }
}