using Massage.Domain.Entities;

namespace Massage.Domain.Events;

public class ReviewAddedEvent(Review review)
{
    public Review Review { get; } = review;
}
