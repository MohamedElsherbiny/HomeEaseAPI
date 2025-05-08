using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Massage.Application.Commands;
using Massage.Application.DTOs;
using Massage.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Massage.Application.Queries.BookingQueries;
using Massage.Application.Commands.BookingCommands;

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> GetById(Guid id)
        {
            var query = new GetBookingByIdQuery { BookingId = id };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BookingDto>>> GetUserBookings(
            [FromQuery] string status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var query = new GetUserBookingsQuery
            {
                UserId = userId,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("provider")]
        [Authorize(Roles = "Provider")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BookingDto>>> GetProviderBookings(
            [FromQuery] string status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var providerId = GetCurrentProviderId();
            var query = new GetProviderBookingsQuery
            {
                ProviderId = providerId,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("provider/statistics")]
        [Authorize(Roles = "Provider")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BookingStatisticsDto>> GetProviderStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var providerId = GetCurrentProviderId();
            var query = new GetBookingStatisticsQuery
            {
                ProviderId = providerId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> CreateBooking([FromBody] CreateBookingRequestDto request)
        {
            var userId = GetCurrentUserId();
            var command = new CreateBookingCommand
            {
                BookingRequest = request,
                UserId = userId
            };

            var bookingId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = bookingId }, bookingId);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateBooking(Guid id, [FromBody] UpdateBookingRequestDto request)
        {
            var userId = GetCurrentUserId();
            var command = new UpdateBookingCommand
            {
                BookingId = id,
                UserId = userId,
                UpdateRequest = request
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("confirm")]
        [Authorize(Roles = "Provider")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ConfirmBooking([FromBody] BookingConfirmationDto request)
        {
            var providerId = GetCurrentProviderId();
            var command = new ConfirmBookingCommand
            {
                ConfirmationRequest = request,
                ProviderId = providerId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CancelBooking([FromBody] BookingCancellationDto request)
        {
            var userId = GetCurrentUserId();
            var isProvider = User.IsInRole("Provider");

            var command = new CancelBookingCommand
            {
                CancellationRequest = request,
                UserId = userId,
                IsProvider = isProvider
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id}/payment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ProcessPayment(Guid id, [FromBody] PaymentInfoDto paymentInfo)
        {
            var command = new ProcessBookingPaymentCommand
            {
                BookingId = id,
                PaymentInfo = paymentInfo
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not authenticated properly");

            return Guid.Parse(userIdClaim.Value);
        }

        private Guid GetCurrentProviderId()
        {
            var providerIdClaim = User.FindFirst("ProviderId");
            if (providerIdClaim == null)
                throw new UnauthorizedAccessException("Provider ID not found in claims");

            return Guid.Parse(providerIdClaim.Value);
        }
    }
}
