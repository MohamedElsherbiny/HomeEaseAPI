using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs.Auth
{
    public class RegisterProviderDto : RegisterUserDto
    {
        [Required]
        public string BusinessName { get; set; }
        public string BusinessNameAr { get; set; }

        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string BusinessAddress { get; set; }

        public string ProfileImageUrl { get; set; }

        public DateTime DateOfBirth { get; set; } 

        public int ExperienceYears { get; set; } 

        public string SpokenLanguage { get; set; } 
    }
}
