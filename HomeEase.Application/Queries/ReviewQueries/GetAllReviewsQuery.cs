using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Queries.ReviewQueries;
using HomeEase.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Queries.ReviewQueries
{
    public class GetAllReviewsQuery : IRequest<PaginatedList<ReviewDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}


public class GetAllReviewsQueryHandler : IRequestHandler<GetAllReviewsQuery, PaginatedList<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetAllReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<ReviewDto>> Handle(GetAllReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetAllAsync(request.PageNumber, request.PageSize);
        var totalCount = reviews.Count();
        var dtos = _mapper.Map<List<ReviewDto>>(reviews);
        return new PaginatedList<ReviewDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}