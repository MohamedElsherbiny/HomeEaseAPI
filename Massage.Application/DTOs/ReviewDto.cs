using System;
using System.ComponentModel.DataAnnotations;

namespace Massage.Application.DTOs
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserProfileImage { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderBusinessName { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string ProviderResponse { get; set; }
        public DateTime? ProviderResponseDate { get; set; }
        public bool IsVerified { get; set; }
    }

    public class CreateReviewDto
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string Comment { get; set; }
    }

    public class ProviderReviewResponseDto
    {
        [Required]
        public Guid ReviewId { get; set; }

        [Required]
        [StringLength(500)]
        public string Response { get; set; }
    }

    public class ReviewSummaryDto
    {
        public Guid ProviderId { get; set; }
        public string ProviderBusinessName { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; }
        public List<ReviewDto> RecentReviews { get; set; }
    }

    public class ReviewFilterDto
    {
        public Guid? ProviderId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ServiceId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? HasComment { get; set; }
        public bool? HasProviderResponse { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}
