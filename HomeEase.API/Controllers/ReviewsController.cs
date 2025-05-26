using HomeEase.Application.Commands.ReviewCommands;
using HomeEase.Application.DTOs;
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
    public class ReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReviewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<Guid>> CreateReview(CreateReviewDto reviewDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var command = new CreateReviewCommand
            {
                UserId = userId,
                ReviewDto = reviewDto
            };
            var reviewId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetReviewById), new { id = reviewId }, reviewId);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewDto>> GetReviewById(Guid id)
        {
            var review = await _mediator.Send(new GetReviewByIdQuery { Id = id });
            if (review == null)
                return NotFound();
            return Ok(review);
        }

        [HttpGet("provider/{providerId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedList<ReviewDto>>> GetReviewsByProviderId(Guid providerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await _mediator.Send(new GetReviewsByProviderIdQuery
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
            var reviews = await _mediator.Send(new GetAllReviewsQuery
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
            var command = new UpdateReviewCommand
            {
                Id = id,
                UserId = userId,
                ReviewDto = reviewDto
            };
            var result = await _mediator.Send(command);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "UserOnly,AdminOnly")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Admin");
            var command = new DeleteReviewCommand
            {
                Id = id,
                UserId = userId,
                IsAdmin = isAdmin
            };
            var result = await _mediator.Send(command);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}