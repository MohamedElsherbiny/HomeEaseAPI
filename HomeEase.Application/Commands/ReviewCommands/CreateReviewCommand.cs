using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ReviewCommands
{
    public class CreateReviewCommand : IRequest<EntityResult>
    {
        public Guid UserId { get; set; }
        public CreateReviewDto ReviewDto { get; set; }
    }

    public class CreateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IBookingRepository bookingRepository,
        IProviderRepository providerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<CreateReviewCommand, EntityResult>
    {
        public async Task<EntityResult> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var booking = await bookingRepository.GetByIdAsync(request.ReviewDto.BookingId);
            if (booking is null)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.BookingNotFound), Messages.BookingNotFound));
            }

            if (booking.UserId != request.UserId)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.BookingUnauthorizedUser), Messages.BookingUnauthorizedUser));
            }

            if (booking.Status != BookingStatus.Completed)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.BookingMustBeCompleted), Messages.BookingMustBeCompleted));
            }

            var existingReview = await reviewRepository.GetByBookingIdAsync(request.ReviewDto.BookingId);
            if (existingReview != null)
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.BookingReviewAlreadyExists), Messages.BookingReviewAlreadyExists));
            }

            if (request.ReviewDto.Rating.HasValue && (request.ReviewDto.Rating < 1.0m || request.ReviewDto.Rating > 5.0m))
            {
                return EntityResult.Failed(new EntityError(nameof(Messages.InvalidReviewRating), Messages.InvalidReviewRating));
            }

            var review = mapper.Map<Review>(request.ReviewDto);

            review.Id = Guid.NewGuid();
            review.UserId = request.UserId;
            review.ProviderId = booking.ProviderId;
            review.CommentAr = request.ReviewDto.CommentAr;
            review.CreatedAt = DateTime.UtcNow;

            await reviewRepository.AddAsync(review);

            var provider = await providerRepository.GetByIdAsync(booking.ProviderId);
            var reviews = await reviewRepository.GetByProviderIdAsync(provider.Id, 1, int.MaxValue);
            var validRatings = reviews.Where(r => r.Rating.HasValue).Select(r => r.Rating.Value).ToList();
            provider.ReviewCount = validRatings.Count;
            provider.Rating = validRatings.Any() ? validRatings.Average() : 0;
            providerRepository.Update(provider);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return EntityResult.SuccessWithData(new { reviewId = review.Id });
        }
    }
}