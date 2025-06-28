using HomeEase.Application.DTOs;
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        //public UserDto User { get; set; }
    }

    public class RegisterResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class RegisterProviderResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
    }

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

    public class PasswordResetRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

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

    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
