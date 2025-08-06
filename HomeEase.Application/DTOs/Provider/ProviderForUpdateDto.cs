using HomeEase.Application.DTOs.ProviderService;

namespace HomeEase.Application.DTOs.Provider;

public class ProviderForUpdateDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool IsActive { get; set; } = true;
    public string BusinessName { get; set; }
    public string BusinessNameAr { get; set; }
    public string Description { get; set; }
    public string DescriptionAr { get; set; }
    public string ProfileImageUrl { get; set; }
    public string? BusinessAddress { get; set; }
    public string? Street { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string Status { get; set; }
    public string? PhoneNumber { get; set; }
    public List<ProviderImageDto> Gallery { get; set; }
    public ProviderImageDto? Logo { get; set; }
    public ProviderImageDto? Cover { get; set; }
    public UserDto User { get; set; }

    // Navigation properties
    public ProviderScheduleDto Schedule { get; set; }
    public List<ServiceDto> Services { get; set; }

}
