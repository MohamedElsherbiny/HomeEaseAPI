using HomeEase.Domain.Enums;

namespace HomeEase.Domain.Entities;

public class Provider
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BusinessName { get; set; }
    public string Description { get; set; }
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

    public virtual User User { get; set; }
    public virtual Address? Address { get; set; }
    public virtual ProviderSchedule? Schedule { get; set; }
    public virtual ICollection<Service> Services { get; set; }
    public virtual ICollection<Location> Locations { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; }
    public virtual ICollection<string> ServiceTypes { get; set; }
    
}
