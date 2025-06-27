using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Infrastructure.Data;

namespace HomeEase.Infrastructure.Services;

public class ProviderService(AppDbContext _context) : IProviderService
{
    public async Task CreateProviderProfile(
        Guid userId,
        string businessName,
        string businessAddress,
        string email,
        string description,
        string profileImageUrl,
        string businessNameAr,
        string descriptionAr,
        int experienceYears,
        string spokenLanguage)
    {
        var provider = new Provider
        {
            Id = userId,
            UserId = userId,
            Email = email,
            BusinessName = businessName,
            BusinessNameAr = businessNameAr,              
            BusinessAddress = businessAddress,
            Description = description,
            DescriptionAr = descriptionAr,               
            ProfileImageUrl = profileImageUrl,
            ExperienceYears = experienceYears,            
            SpokenLanguage = spokenLanguage,              
            Status = ProviderStatus.Pending,
            Rating = 0
        };

        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
    }
}

