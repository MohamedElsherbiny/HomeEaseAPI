using HomeEase.Domain.Enums;

namespace HomeEase.Domain.Entities;

public class Service
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Name { get; set; }
    public string? NameAr { get; set; }
    public string Description { get; set; }
    public string? DescriptionAr { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public decimal HomePrice { get; set; }
    public decimal Rating { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid BasePlatformServiceId { get; set; }
    public virtual Provider Provider { get; set; }
    public virtual BasePlatformService BasePlatformService { get; set; }
}