using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Infrastructure.Data;

namespace HomeEase.Infrastructure.Services;

public class ProviderService(AppDbContext _context) : IProviderService
{
    public async Task CreateProviderProfile(
        User user,
        string businessName,
        string businessAddress,
        string email,
        string description,
        string profileImageUrl,
        string businessNameAr,
        string descriptionAr,
        int experienceYears,
        string spokenLanguage,
        string street,
        string logoUrl,
        string coverUrl,
        List<string> images,
        decimal? lat,
        decimal? lng)
    {
        var provider = new Provider
        {
            Id = user.Id,
            UserId = user.Id,
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

        if (lat.HasValue && lng.HasValue)
        {
            provider.Address = new Address
            {
                Latitude = lat,
                Longitude = lng.Value,
                CreatedAt = DateTime.UtcNow,
                City = "",
                Street = street ?? "",
                State = "",
                PostalCode = "",
                Country = "",
                ZipCode = "",
                User = user
            };
        }

        var providerImages = new List<ProviderImage>();

        if (!string.IsNullOrEmpty(logoUrl))
        {
            providerImages.Add(new ProviderImage
            {
                Id = Guid.NewGuid(),
                ProviderId = provider.Id,
                ImageUrl = logoUrl,
                ImageType = ImageType.Logo
            });
        }

        if (!string.IsNullOrEmpty(coverUrl))
        {
            providerImages.Add(new ProviderImage
            {
                Id = Guid.NewGuid(),
                ProviderId = provider.Id,
                ImageUrl = coverUrl,
                ImageType = ImageType.Cover
            });
        }

        if (images != null && images.Any())
        {
            int order = 0;
            foreach (var img in images)
            {
                providerImages.Add(new ProviderImage
                {
                    Id = Guid.NewGuid(),
                    ProviderId = provider.Id,
                    ImageUrl = img,
                    ImageType = ImageType.Gallery,
                    SortOrder = order++
                });
            }
        }

        provider.Images = providerImages;

        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
    }
}

