namespace HomeEase.Domain.Entities;

public class BasePlatformService
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? NameAr { get; set; }
    public string Description { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; } = true;
    public string ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}