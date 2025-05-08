using Massage.Application.Commands.AuthCommands;
using Massage.Application.DTOs;
using Massage.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace Massage.Application.Commands.AuthCommands
{
    public class ResetPasswordCommand : IRequest<bool>
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }

        public ResetPasswordCommand(PasswordResetDto dto)
        {
            Token = dto.Token;
            Email = dto.Email;
            NewPassword = dto.NewPassword;
        }
    }
}


// Command Handler
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly UserManager<User> _userManager;

    public ResetPasswordCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new ApplicationException("User not found.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            throw new ApplicationException($"Password reset failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        return true;
    }
}
