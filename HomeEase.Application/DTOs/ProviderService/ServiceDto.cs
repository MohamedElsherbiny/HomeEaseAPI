namespace HomeEase.Application.DTOs.ProviderService;

public class ServiceDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid BasePlatformServiceId { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal HomePrice { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsAvailableAtCenter => Price != 0;
    public bool IsAvailableAtHome => HomePrice != 0;
}
