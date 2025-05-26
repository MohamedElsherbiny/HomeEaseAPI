using HomeEase.Application.Commands.AuthCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace HomeEase.Application.Commands.AuthCommands
{
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
}


// Command Handler
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
            Role = HomeEase.Domain.Enums.UserRole.Provider,
            RefreshToken = "",
            ProfileImageUrl = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ApplicationException($"Provider registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        //await _userManager.AddToRoleAsync(user, "Provider");

        // Create provider profile
        await _providerService.CreateProviderProfile(user.Id, request.BusinessName, request.BusinessAddress, request.Email, request.Description, request.ProfileImageUrl, request.ServiceTypes);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            ProfileImageUrl = "",
            IsActive = user.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return userDto;
    }
}