using HomeEase.Application.DTOs.Common;
using HomeEase.Domain.Entities;
using HomeEase.Resources;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace HomeEase.Application.Commands.AuthCommands;

public class VerifyOtpCommand : IRequest<EntityResult>
{
    public string Email { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
}

public class VerifyOtpCommandHandler(UserManager<User> _userManager) : IRequestHandler<VerifyOtpCommand, EntityResult>
{
    public async Task<EntityResult> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UserNotFoundByEmail), string.Format(Messages.UserNotFoundByEmail, request.Email)));
        }

        var isValid = await _userManager.VerifyUserTokenAsync(user, "OtpProvider", "ResetPassword", request.OtpCode);

        if (!isValid)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.InvalidOrExpiredOtp), Messages.InvalidOrExpiredOtp));
        }

        return EntityResult.SuccessWithData(new
        {
            IsValid = true,
            Message = Messages.ValidOtp,
        });
    }
}