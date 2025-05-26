using System;
using System.ComponentModel.DataAnnotations;

namespace HomeEase.Application.DTOs
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid ProviderId { get; set; }
        public decimal? Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserName { get; set; } 
    }

    public class CreateReviewDto
    {
        public Guid BookingId { get; set; }
        public decimal? Rating { get; set; }
        public string Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        public decimal? Rating { get; set; }
        public string Comment { get; set; }
    }
}
