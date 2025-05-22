using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Infrastructure.Data;

namespace HomeEase.Infrastructure.Services;

public class ProviderService(AppDbContext _context) : IProviderService
{
    public async Task CreateProviderProfile(Guid userId, string businessName, string businessAddress, string Email, string description, string profileImageUrl, string[] serviceTypes)
    {
        var provider = new Provider
        {
            Id = userId,
            UserId = userId,
            Email = Email,
            BusinessName = businessName,
            BusinessAddress = businessAddress,
            Description = description,
            ProfileImageUrl = profileImageUrl,
            ServiceTypes = serviceTypes.ToList(),
            Status = ProviderStatus.Pending,
            Rating = 0
        };

        _context.Providers.Add(provider);

        await _context.SaveChangesAsync();
    }
}
