namespace HomeEase.Domain.Entities;

public class BasePlatformService
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string NameAr { get; set; }
    public bool IsActive { get; set; } = true;
    public string ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}