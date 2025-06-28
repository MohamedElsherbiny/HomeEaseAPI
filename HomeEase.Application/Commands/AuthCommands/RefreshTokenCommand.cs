using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using HomeEase.Application.Commands.AuthCommands;


namespace HomeEase.Application.Commands.AuthCommands
{
    public class RefreshTokenCommand : IRequest<LoginResponseDto>
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public RefreshTokenCommand(RefreshTokenDto dto)
        {
            Token = dto.Token;
            RefreshToken = dto.RefreshToken;
        }
    }
}


// Command Handler
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userService;

    public RefreshTokenCommandHandler(UserManager<User> userManager, IJwtService jwtService, IUserRepository userService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _userService = userService;
    }

    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.Token);
        var username = principal.Identity.Name;

        var user = await _userManager.FindByNameAsync(username);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new SecurityTokenException("Invalid refresh token");

        var roles = await _userManager.GetRolesAsync(user);
        var newToken = _jwtService.GenerateToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
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
            Token = newToken.Token,
            RefreshToken = newRefreshToken,
            Expiration = newToken.Expiration
        };
    }
}