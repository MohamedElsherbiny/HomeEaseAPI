using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Repositories;
using MediatR;
using System.Security.Authentication;
using Microsoft.AspNetCore.Identity;
using HomeEase.Domain.Enums;

namespace HomeEase.Application.Commands.AuthCommands
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

    public class LoginCommandHandler(IProviderRepository _providerRepository, IJwtService _jwtService, UserManager<User> _userManager)
        : IRequestHandler<LoginCommand, LoginResponseDto>
    {

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new AuthenticationException("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                throw new AuthenticationException("Account is deactivated. Please contact support.");
            }

            Provider? provider = null;
            bool profileCompleted = true;
            if (user.Role == UserRole.Provider)
            {
                 provider = await _providerRepository.GetByUserIdAsync(user.Id);
                if (provider == null)
                {
                    throw new AuthenticationException("Provider profile not found.");
                }

                if (!provider.IsActive)
                {
                    throw new AuthenticationException("Provider account is deactivated. Please contact support.");
                }
                profileCompleted = provider.ProfileCompleted; 
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                throw new AuthenticationException("Invalid email or password.");

            var roles = new List<string> { user.Role.ToString() };

            var (Token, Expiration) = _jwtService.GenerateToken(user, roles, provider);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new LoginResponseDto
            {
                Token = Token,
                RefreshToken = refreshToken,
                Expiration = Expiration,
                ProfileCompleted = profileCompleted,
                Role = user.Role.ToString()
            };
        }
    }
}