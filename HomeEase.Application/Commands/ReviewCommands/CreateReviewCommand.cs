using AutoMapper;
using HomeEase.Application.Commands.ReviewCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Repositories;
using MediatR;

namespace HomeEase.Application.Commands.ReviewCommands
{
    public class CreateReviewCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public CreateReviewDto ReviewDto { get; set; }
    }

    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Guid>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateReviewCommandHandler(
            IReviewRepository reviewRepository,
            IBookingRepository bookingRepository,
            IProviderRepository providerRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _bookingRepository = bookingRepository;
            _providerRepository = providerRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.ReviewDto.BookingId);
            if (booking == null)
                throw new ApplicationException("Booking not found.");

            if (booking.UserId != request.UserId)
                throw new ApplicationException("User is not authorized to review this booking.");

            if (booking.Status != BookingStatus.Completed)
                throw new ApplicationException("Booking must be completed to leave a review.");

            var existingReview = await _reviewRepository.GetByBookingIdAsync(request.ReviewDto.BookingId);
            if (existingReview != null)
                throw new ApplicationException("A review already exists for this booking.");

            if (request.ReviewDto.Rating.HasValue && (request.ReviewDto.Rating < 1.0m || request.ReviewDto.Rating > 5.0m))
                throw new ApplicationException("Rating must be between 1.0 and 5.0.");

            var review = _mapper.Map<Review>(request.ReviewDto);
            review.Id = Guid.NewGuid();
            review.UserId = request.UserId;
            review.ProviderId = booking.ProviderId;
            review.CommentAr = request.ReviewDto.CommentAr;
            review.CreatedAt = DateTime.UtcNow;

            await _reviewRepository.AddAsync(review);

            // Update provider's rating and review count
            var provider = await _providerRepository.GetByIdAsync(booking.ProviderId);
            var reviews = await _reviewRepository.GetByProviderIdAsync(provider.Id, 1, int.MaxValue);
            var validRatings = reviews.Where(r => r.Rating.HasValue).Select(r => r.Rating.Value).ToList();
            provider.ReviewCount = validRatings.Count;
            provider.Rating = validRatings.Any() ? validRatings.Average() : 0;
            _providerRepository.Update(provider);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return review.Id;
        }
    }
}