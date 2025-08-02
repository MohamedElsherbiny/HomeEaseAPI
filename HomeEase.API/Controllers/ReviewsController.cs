using HomeEase.Application.Commands.ReviewCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Queries.ReviewQueries;
using HomeEase.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HomeEase.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewsController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<Guid>> CreateReview(CreateReviewDto reviewDto)
        {
            return Ok(await mediator.Send(new CreateReviewCommand
            {
                UserId = currentUserService.UserId,
                ReviewDto = reviewDto
            }));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewDto>> GetReviewById(Guid id)
        {
            var review = await mediator.Send(new GetReviewByIdQuery { Id = id });
            if (review == null)
                return NotFound();
            return Ok(review);
        }

        [HttpGet("provider/{providerId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedList<ReviewDto>>> GetReviewsByProviderId(Guid providerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await mediator.Send(new GetReviewsByProviderIdQuery
            {
                ProviderId = providerId,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            return Ok(reviews);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedList<ReviewDto>>> GetAllReviews([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await mediator.Send(new GetAllReviewsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            return Ok(reviews);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> UpdateReview(Guid id, UpdateReviewDto reviewDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Ok(await mediator.Send(new UpdateReviewCommand
            {
                Id = id,
                UserId = userId,
                ReviewDto = reviewDto
            }));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "UserOnly,AdminOnly")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Admin");

            return Ok(await mediator.Send(new DeleteReviewCommand
            {
                Id = id,
                UserId = userId,
                IsAdmin = isAdmin
            }));
        }
    }
}