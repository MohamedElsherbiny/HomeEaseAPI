using HomeEase.Application.Commands.AuthCommands;
using HomeEase.Application.DTOs.Auth;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Data;



namespace HomeEase.Application.Commands.AuthCommands;

public class RegisterUserCommand : IRequest<RegisterResponseDto>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }

    public RegisterUserCommand(RegisterUserDto dto)
    {
        Email = dto.Email;
        Password = dto.Password;
        FirstName = dto.FirstName;
        LastName = dto.LastName;
        PhoneNumber = dto.PhoneNumber;
    }
}

public class RegisterUserCommandHandler(UserManager<User> _userManager, IUserRepository _userService, IJwtService _jwtService) : IRequestHandler<RegisterUserCommand, RegisterResponseDto>
{
    public async Task<RegisterResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ApplicationException("User with this email already exists.");

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = false,
            ProfileImageUrl = "",
            RefreshToken = "",
            Role = HomeEase.Domain.Enums.UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ApplicationException($"User registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");


        var roles = new List<string> { user.Role.ToString() };
        var (Token, Expiration) = _jwtService.GenerateToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var response = new RegisterResponseDto
        {
            Token = Token,
            RefreshToken = refreshToken,
            Expiration = Expiration
        };

        return response;
    }
}