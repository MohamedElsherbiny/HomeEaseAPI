using HomeEase.Domain.Enums;

namespace HomeEase.Domain.Entities;

public class Provider
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BusinessName { get; set; }
    public string? BusinessNameAr { get; set; }
    public string Description { get; set; }
    public string? DescriptionAr { get; set; }
    public int ReviewCount { get; set; }
    public decimal Rating { get; set; }
    public ProviderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeactivatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? VerifiedAt { get; set; }
    public string ProfileImageUrl { get; set; }
    public Guid? AddressId { get; set; }
    public string Email { get; set; }
    public string BusinessAddress { get; set; }
    public int ExperienceYears { get; set; }         
    public string SpokenLanguage { get; set; }

    public virtual User User { get; set; }
    public virtual Address? Address { get; set; }
    public virtual ProviderSchedule? Schedule { get; set; }
    public virtual ICollection<Service> Services { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; }
    public virtual ICollection<ProviderImage> Images { get; set; } = new List<ProviderImage>();

}

public class ProviderImage
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string ImageUrl { get; set; }
    public ImageType ImageType { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Provider Provider { get; set; }
}

public enum ImageType
{
    Gallery,
    Logo,
    Cover
}