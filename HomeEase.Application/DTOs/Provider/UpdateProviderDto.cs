namespace HomeEase.Application.DTOs.Provider;

public class UpdateProviderDto
{
    public string? BusinessName { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? BusinessAddress { get; set; }
    public string? Street { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public AddressDto? Address { get; set; }
}
