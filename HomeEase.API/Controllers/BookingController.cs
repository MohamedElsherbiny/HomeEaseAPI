using HomeEase.Application.Commands.BookingCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.DTOs.Booking;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Queries.BookingQueries;
using HomeEase.Domain.Common;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController(IMediator _mediator, ICurrentUserService _currentUserService) : ControllerBase
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
    public async Task<ActionResult<PaginatedList<BookingDto>>> GetUserBookings([FromQuery] GetUserBookingsQuery query)
    {
        query.UserId = _currentUserService.UserId;

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("provider")]
    [Authorize(Policy = "ProviderOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<BookingDto>>> GetProviderBookings([FromQuery] GetProviderBookingsQuery query)
    {
        var providerId = GetCurrentProviderId();

        query.ProviderId = providerId;

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
                Name = EnumTranslations.TranslateBookingStatus(status, _currentUserService.Language)
            })
            .ToList();

        return Ok(statuses);
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateBooking([FromBody] CreateBookingRequestDto request)
    {
        return Ok(await _mediator.Send(new CreateBookingCommand
        {
            BookingRequest = request,
            UserId = _currentUserService.UserId
        }));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateBooking(Guid id, [FromBody] UpdateBookingRequestDto request)
    {
        var command = new UpdateBookingCommand
        {
            BookingId = id,
            UserId = _currentUserService.UserId,
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
        var isProvider = User.IsInRole("Provider");

        var command = new CancelBookingCommand
        {
            CancellationRequest = request,
            UserId = _currentUserService.UserId,
            IsProvider = isProvider
        };

        return Ok(await _mediator.Send(command));
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

    private Guid GetCurrentProviderId()
    {
        var providerIdClaim = User.FindFirst("ProviderId");
        if (providerIdClaim == null)
            throw new UnauthorizedAccessException("Provider ID not found in claims");

        return Guid.Parse(providerIdClaim.Value);
    }
}
