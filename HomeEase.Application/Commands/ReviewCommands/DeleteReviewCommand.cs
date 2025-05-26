using HomeEase.Application.Commands.ReviewCommands;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Commands.ReviewCommands
{
    public class DeleteReviewCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool IsAdmin { get; set; }
    }
}


public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, bool>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReviewCommandHandler(
        IReviewRepository reviewRepository,
        IProviderRepository providerRepository,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.Id);
        if (review == null)
            return false;

        if (!request.IsAdmin && review.UserId != request.UserId)
            throw new ApplicationException("User is not authorized to delete this review.");

        _reviewRepository.Delete(review);

        // Update provider's rating and review count
        var provider = await _providerRepository.GetByIdAsync(review.ProviderId);
        var reviews = await _reviewRepository.GetByProviderIdAsync(provider.Id, 1, int.MaxValue);
        var validRatings = reviews.Where(r => r.Rating.HasValue).Select(r => r.Rating.Value).ToList();
        provider.ReviewCount = validRatings.Count;
        provider.Rating = validRatings.Any() ? validRatings.Average() : 0;
        _providerRepository.Update(provider);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}