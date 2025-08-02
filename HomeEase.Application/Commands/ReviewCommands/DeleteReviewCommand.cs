using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ReviewCommands;

public class DeleteReviewCommand : IRequest<EntityResult>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool IsAdmin { get; set; }
}

public class DeleteReviewCommandHandler(
    IReviewRepository reviewRepository,
    IProviderRepository providerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteReviewCommand, EntityResult>
{
    public async Task<EntityResult> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.Id);
        if (review is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.ReviewNotFound), Messages.ReviewNotFound));
        }

        if (!request.IsAdmin && review.UserId != request.UserId)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UnauthorizedReviewDelete), Messages.UnauthorizedReviewDelete));
        }

        reviewRepository.Delete(review);

        var provider = await providerRepository.GetByIdAsync(review.ProviderId);
        var reviews = await reviewRepository.GetByProviderIdAsync(provider.Id, 1, int.MaxValue);
        var validRatings = reviews.Where(r => r.Rating.HasValue).Select(r => r.Rating.Value).ToList();
        provider.ReviewCount = validRatings.Count;
        provider.Rating = validRatings.Count != 0 ? validRatings.Average() : 0;

        providerRepository.Update(provider);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}