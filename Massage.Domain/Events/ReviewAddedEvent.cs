using Massage.Domain.Entities;
using Massage.Domain.Entities;

namespace Massage.Domain.Events
{
    public class ReviewAddedEvent
    {
        public Review Review { get; }

        public ReviewAddedEvent(Review review)
        {
            Review = review;
        }
    }
}
