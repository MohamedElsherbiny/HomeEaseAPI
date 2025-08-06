namespace HomeEase.Application.DTOs.ProviderService
{
    public class CreateServiceDto
    {
        public Guid BasePlatformServiceId { get; set; }
        public decimal Price { get; set; }
        public decimal HomePrice { get; set; }

    }
}
