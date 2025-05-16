using Massage.Application.DTOs;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using MediatR;
using System.Security.Authentication;
using Microsoft.AspNetCore.Identity;
using Massage.Application.Commands.AuthCommands;
using Massage.Domain.Enums;

namespace Massage.Application.Commands.AuthCommands
{
    public class LoginCommand : IRequest<LoginResponseDto>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public LoginCommand(LoginRequestDto dto)
        {
            Email = dto.Email;
            Password = dto.Password;
        }
    }
}


// Command Handler
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _userService;
    private readonly IJwtService _jwtService;
    private readonly UserManager<User> _userManager;

    public LoginCommandHandler(IUserRepository userService, IJwtService jwtService, UserManager<User> userManager)
    {
        _userService = userService;
        _jwtService = jwtService;
        _userManager = userManager;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new AuthenticationException("Invalid email or password.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            throw new AuthenticationException("Invalid email or password.");

 
        var roles = new List<string> { user.Role.ToString() };


        var token = _jwtService.GenerateToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            ProfileImageUrl = user.ProfileImageUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.UpdatedAt
        };

        return new LoginResponseDto
        {
            Token = token.Token,
            RefreshToken = refreshToken,
            Expiration = token.Expiration,
            User = userDto
        };
    }
}