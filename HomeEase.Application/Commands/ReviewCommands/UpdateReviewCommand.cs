using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using HomeEase.Resources;
using MediatR;

namespace HomeEase.Application.Commands.ReviewCommands;

public class UpdateReviewCommand : IRequest<EntityResult>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UpdateReviewDto ReviewDto { get; set; }
}

public class UpdateReviewCommandHandler(
    IReviewRepository reviewRepository,
    IProviderRepository providerRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateReviewCommand, EntityResult>
{
    public async Task<EntityResult> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.Id);
        if (review is null)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.ReviewNotFound), Messages.ReviewNotFound));
        }

        if (review.UserId != request.UserId)
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.UnauthorizedReviewUpdate), Messages.UnauthorizedReviewUpdate));

        }
        if (request.ReviewDto.Rating.HasValue && (request.ReviewDto.Rating < 1.0m || request.ReviewDto.Rating > 5.0m))
        {
            return EntityResult.Failed(new EntityError(nameof(Messages.InvalidReviewRating), Messages.InvalidReviewRating));
        }
        mapper.Map(request.ReviewDto, review);
        review.UpdatedAt = DateTime.UtcNow;

        reviewRepository.Update(review);

        // Update provider's rating and review count
        var provider = await providerRepository.GetByIdAsync(review.ProviderId);
        var reviews = await reviewRepository.GetByProviderIdAsync(provider.Id, 1, int.MaxValue);
        var validRatings = reviews.Where(r => r.Rating.HasValue).Select(r => r.Rating.Value).ToList();
        provider.ReviewCount = validRatings.Count;
        provider.Rating = validRatings.Any() ? validRatings.Average() : 0;
        providerRepository.Update(provider);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityResult.Success;
    }
}