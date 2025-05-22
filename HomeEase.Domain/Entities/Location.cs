namespace HomeEase.Domain.Entities;

public class Location
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid AddressId { get; set; }
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsProviderOffice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Provider Provider { get; set; }
    public virtual Address Address { get; set; }
}
