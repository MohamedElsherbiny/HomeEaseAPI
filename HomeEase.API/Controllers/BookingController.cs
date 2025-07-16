using HomeEase.Application.Commands.BookingCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Queries.BookingQueries;
using HomeEase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController(IMediator _mediator) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBookingByIdQuery { BookingId = id });

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
    [Authorize(Policy = "ProviderOnly")]
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

    [HttpGet("booking-statuses")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<BookingStatusDto>> GetBookingStatuses()
    {
        var statuses = Enum.GetValues(typeof(BookingStatus))
            .Cast<BookingStatus>()
            .Select(status => new BookingStatusDto
            {
                Id = (int)status,
                Name = status.ToString(),
                NameAr = GetArabicTranslation(status)
            })
            .ToList();

        return Ok(statuses);
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
        return Ok(new { bookingId });
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

    [HttpPost("payment/{id}")]
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

    private string GetArabicTranslation(BookingStatus status)
    {
        return status switch
        {
            BookingStatus.Pending => "قيد الانتظار",
            BookingStatus.Confirmed => "تم التأكيد",
            BookingStatus.Completed => "مكتمل",
            BookingStatus.Cancelled => "ألغيت",
            BookingStatus.Rejected => "مرفوض",
            _ => string.Empty
        };
    }

    public class BookingStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
    }
}
