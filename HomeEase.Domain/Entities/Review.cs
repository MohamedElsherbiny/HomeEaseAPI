namespace HomeEase.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProviderId { get; set; }
    public decimal? Rating { get; set; }
    public string Comment { get; set; }
    public string? CommentAr { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual Booking Booking { get; set; }
    public virtual User User { get; set; }
    public virtual Provider Provider { get; set; }
}
