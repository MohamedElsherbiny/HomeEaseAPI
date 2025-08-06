using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs.Auth
{
    public class PasswordResetRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
