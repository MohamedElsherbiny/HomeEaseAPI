using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs.ProviderService
{
    public class BlukUpdateServicesDto 
    {
        [Required] 
        [MinLength(1, ErrorMessage = "At least one service must be provided.")]
        public List<CreateServiceDto> Services { get; set; } = [];
    }
}
