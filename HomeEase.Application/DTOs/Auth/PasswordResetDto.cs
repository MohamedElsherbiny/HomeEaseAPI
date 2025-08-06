using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs.Auth
{
    public class PasswordResetDto
    {
        [Required]
        public string OtpCode { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
