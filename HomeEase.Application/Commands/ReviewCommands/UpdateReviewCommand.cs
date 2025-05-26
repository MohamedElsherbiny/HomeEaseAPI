using AutoMapper;
using HomeEase.Application.Commands.ReviewCommands;
using HomeEase.Application.DTOs;
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
    public class UpdateReviewCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UpdateReviewDto ReviewDto { get; set; }
    }
}


public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, bool>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IProviderRepository providerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<bool> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.Id);
        if (review == null)
            return false;

        if (review.UserId != request.UserId)
            throw new ApplicationException("User is not authorized to update this review.");

        if (request.ReviewDto.Rating.HasValue && (request.ReviewDto.Rating < 1.0m || request.ReviewDto.Rating > 5.0m))
            throw new ApplicationException("Rating must be between 1.0 and 5.0.");

        _mapper.Map(request.ReviewDto, review);
        review.UpdatedAt = DateTime.UtcNow;

        _reviewRepository.Update(review);

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