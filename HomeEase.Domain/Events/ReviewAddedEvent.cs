using HomeEase.Domain.Entities;

namespace HomeEase.Domain.Events;

public class ReviewAddedEvent(Review review)
{
    public Review Review { get; } = review;
}
