using HomeEase.Application.Commands.PaymentCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Queries.BookingQueries;
using HomeEase.Application.Queries.PaymentQueries;
using HomeEase.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeEase.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<PaymentResultDto>> CreatePayment(CreatePaymentDto paymentDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var command = new CreatePaymentCommand
            {
                UserId = userId,
                PaymentDto = paymentDto
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "UserOnly,ProviderOnly,AdminOnly")]
        public async Task<ActionResult<PaymentInfoDto>> GetPaymentById(Guid id)
        {
            var payment = await _mediator.Send(new GetPaymentByIdQuery { Id = id });
            if (payment == null)
                return NotFound();

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Admin");
            var booking = await _mediator.Send(new GetBookingByIdQuery { BookingId = payment.BookingId }); // Assume this query exists
            if (!isAdmin && booking.UserId != userId && booking.ProviderId != userId)
                return Forbid();

            return Ok(payment);
        }

        [HttpGet("booking/{bookingId}")]
        [Authorize(Policy = "UserOnly,ProviderOnly,AdminOnly")]
        public async Task<ActionResult<IEnumerable<PaymentInfoDto>>> GetPaymentsByBookingId(Guid bookingId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Admin");
            var booking = await _mediator.Send(new GetBookingByIdQuery { BookingId = bookingId });
            if (!isAdmin && booking.UserId != userId && booking.ProviderId != userId)
                return Forbid();

            var payments = await _mediator.Send(new GetPaymentsByBookingIdQuery { BookingId = bookingId });
            return Ok(payments);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PaginatedList<PaymentInfoDto>>> GetAllPayments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var payments = await _mediator.Send(new GetAllPaymentsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            return Ok(payments);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdatePayment(Guid id, UpdatePaymentDto paymentDto)
        {
            var result = await _mediator.Send(new UpdatePaymentCommand
            {
                Id = id,
                PaymentDto = paymentDto
            });
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeletePayment(Guid id)
        {
            var result = await _mediator.Send(new DeletePaymentCommand { Id = id });
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/refund")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<PaymentResultDto>> RefundPayment(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _mediator.Send(new RefundPaymentCommand
            {
                Id = id,
                UserId = userId
            });
            return Ok(result);
        }
    }
}
