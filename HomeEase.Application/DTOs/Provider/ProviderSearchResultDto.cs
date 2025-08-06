namespace HomeEase.Application.DTOs.Provider;

public class ProviderSearchResultDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; }
    public string ProfileImageUrl { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public AddressDto Address { get; set; }
    public decimal? Distance { get; set; }
    public List<ProviderImageDto> Gallery { get; set; }
    public ProviderImageDto? Logo { get; set; }
    public ProviderImageDto? Cover { get; set; }
}
