using HomeEase.Application.Commands.AuthCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;



namespace HomeEase.Application.Commands.AuthCommands
{
    public class RequestPasswordResetCommand : IRequest<bool>
    {
        public string Email { get; set; }

        public RequestPasswordResetCommand(PasswordResetRequestDto dto)
        {
            Email = dto.Email;
        }
    }
}


// Command Handler
public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;

    public RequestPasswordResetCommandHandler(UserManager<User> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<bool> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return true; // Return true even if user not found for security reasons

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(user.Email, token);

        return true;
    }
}