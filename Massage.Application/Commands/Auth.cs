using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Massage.Application.Commands;
using Massage.Application.DTOs;
using Massage.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


// Commands
using Massage.Application.DTOs;
using Massage.Application.Commands;
using MediatR;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Claims;
using Massage.Application.Interfaces.Services;

namespace Massage.Application.Commands
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

    public class RegisterUserCommand : IRequest<UserDto>
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

    public class RegisterProviderCommand : IRequest<UserDto>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string BusinessName { get; set; }
        public string Description { get; set; }
        public string ProfileImageUrl { get; set; }
        public string[] ServiceTypes { get; set; }
        public string BusinessAddress { get; set; }


        public RegisterProviderCommand(RegisterProviderDto dto)
        {
            Email = dto.Email;
            Password = dto.Password;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            PhoneNumber = dto.PhoneNumber;
            BusinessName = dto.BusinessName;
            BusinessAddress = dto.BusinessAddress;
            Description = dto.Description;
            ProfileImageUrl = dto.ProfileImageUrl;
            ServiceTypes = dto.ServiceTypes;
        }
    }

    public class RequestPasswordResetCommand : IRequest<bool>
    {
        public string Email { get; set; }

        public RequestPasswordResetCommand(PasswordResetRequestDto dto)
        {
            Email = dto.Email;
        }
    }

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

// Command Handlers
namespace Massage.Application.Handlers
{
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

            var roles = await _userManager.GetRolesAsync(user);

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

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
    {
        private readonly UserManager<User> _userManager; // Corrected type
        private readonly IUserRepository _userService;

        public RegisterUserCommandHandler(UserManager<User> userManager, IUserRepository userService) // Corrected type
        {
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
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
                Role = Domain.Enums.UserRole.User
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new ApplicationException($"User registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                ProfileImageUrl = user.ProfileImageUrl
            };

            return userDto;
        }
    }

    public class RegisterProviderCommandHandler : IRequestHandler<RegisterProviderCommand, UserDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userService;
        private readonly IProviderService _providerService;

        public RegisterProviderCommandHandler(UserManager<User> userManager, IUserRepository userService, IProviderService providerService)
        {
            _userManager = userManager;
            _userService = userService;
            _providerService = providerService;
        }

        public async Task<UserDto> Handle(RegisterProviderCommand request, CancellationToken cancellationToken)
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
                Role = Domain.Enums.UserRole.Provider,
                RefreshToken = "",
                ProfileImageUrl = ""
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new ApplicationException($"Provider registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            //await _userManager.AddToRoleAsync(user, "Provider");
          
            // Create provider profile
            await _providerService.CreateProviderProfile(user.Id, request.BusinessName,request.BusinessAddress , request.Email, request.Description, request.ProfileImageUrl, request.ServiceTypes);

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                ProfileImageUrl = ""
            };

            return userDto;
        }
    }

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
                Expiration = newToken.Expiration,
                User = userDto
            };
        }
    }
}




