namespace HomeEase.Domain.Entities;

public class UserServiceLike
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; }
    public virtual Service Service { get; set; }
}