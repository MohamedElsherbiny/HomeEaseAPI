﻿using System;
using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string Role { get; set; }

        public bool IsActive { get; set; }
        public string? ProviderId { get; set; }
        public bool ProfileCompleted { get; set; }
    }

    public class UpdateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }
    }

    public class UserPreferencesDto
    {
        public Guid UserId { get; set; }
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public string PreferredCurrency { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class AddressDto
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string ZipCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }

    public class UserServiceLikeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


