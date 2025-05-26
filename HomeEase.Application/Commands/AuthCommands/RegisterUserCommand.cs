using HomeEase.Application.Commands.AuthCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;



namespace HomeEase.Application.Commands.AuthCommands
{
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
}


// Command Handler
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
            Role = HomeEase.Domain.Enums.UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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
            ProfileImageUrl = user.ProfileImageUrl,
            IsActive = user.IsActive
        };

        return userDto;
    }
}