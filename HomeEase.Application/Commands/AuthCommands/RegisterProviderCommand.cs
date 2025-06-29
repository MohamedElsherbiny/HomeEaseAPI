using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace HomeEase.Application.Commands.AuthCommands;

public class RegisterProviderCommand : IRequest<RegisterProviderResponseDto>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string BusinessName { get; set; }
    public string BusinessNameAr { get; set; }
    public string Description { get; set; }
    public string DescriptionAr { get; set; }
    public int ExperienceYears { get; set; }
    public string SpokenLanguage { get; set; }
    public string ProfileImageUrl { get; set; }
    public string BusinessAddress { get; set; }
    public string LogoUrl { get; set; }
    public string CoverUrl { get; set; }
    public string Street { get; set; }
    public List<string> Images { get; set; } = new();
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class RegisterProviderCommandHandler(UserManager<User> _userManager, IJwtService _jwtService, IProviderService _providerService) : IRequestHandler<RegisterProviderCommand, RegisterProviderResponseDto>
{
    public async Task<RegisterProviderResponseDto> Handle(RegisterProviderCommand request, CancellationToken cancellationToken)
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
            UpdatedAt = DateTime.UtcNow,
            DateOfBirth = request.DateOfBirth
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ApplicationException($"Provider registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await _providerService.CreateProviderProfile(
            user,
            request.BusinessName,
            request.BusinessAddress,
            request.Email,
            request.Description,
            request.ProfileImageUrl,
            request.BusinessNameAr,
            request.DescriptionAr,
            request.ExperienceYears,
            request.SpokenLanguage,
            request.Street,
            request.LogoUrl,
            request.CoverUrl,
            request.Images,
            request.Latitude,
            request.Longitude
        );

        var roles = new List<string> { user.Role.ToString() };
        var (Token, Expiration) = _jwtService.GenerateToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var response = new RegisterProviderResponseDto
        {
            Token = Token,
            RefreshToken = refreshToken,
            Expiration = Expiration
        };

        return response;
    }
}