using HomeEase.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace HomeEase.Domain.Entities;

public class User : IdentityUser<Guid> 
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public string ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; }
    public virtual ICollection<Review> Reviews { get; set; }
    public virtual ICollection<Address> Addresses { get; set; }
    public virtual UserServiceLike Preferences { get; set; }
    
}