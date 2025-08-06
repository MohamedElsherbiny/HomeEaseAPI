using HomeEase.Application.DTOs;
using HomeEase.Application.DTOs.Auth;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.AdminCommands
{
    // Command to create a new Admin
    public class CreateAdminCommand : IRequest<UserDto>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        public CreateAdminCommand()
        {
        }

        public CreateAdminCommand(RegisterUserDto dto)
        {
            Email = dto.Email;
            Password = dto.Password;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            PhoneNumber = dto.PhoneNumber;
        }
    }

    public class CreateAdminCommandHandler : IRequestHandler<CreateAdminCommand, UserDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        public CreateAdminCommandHandler(UserManager<User> userManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
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
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new ApplicationException($"Admin creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                ProfileImageUrl = user.ProfileImageUrl
            };
        }
    }
}
