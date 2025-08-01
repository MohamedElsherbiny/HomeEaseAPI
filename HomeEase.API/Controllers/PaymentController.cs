using HomeEase.Application.Commands.BookingCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Queries.BookingQueries;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HomeEase.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IMediator mediator, ILogger<PaymentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<ActionResult<PaymentResponseDto>> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var command = new ProcessBookingPaymentCommand
                {
                    BookingId = request.BookingId,
                    PaymentInfo = request.PaymentInfo,
                    Customer = new CustomerInfo
                    {
                        FirstName = request.PaymentInfo.Customer.FirstName,
                        LastName = request.PaymentInfo.Customer.LastName,
                        Email = request.PaymentInfo.Customer.Email,
                        PhoneNumber = request.PaymentInfo.Customer.PhoneNumber,
                        PhoneCountryCode = request.PaymentInfo.Customer.PhoneCountryCode
                    }
                };

                var result = await _mediator.Send(command);

                return Ok(new PaymentResponseDto
                {
                    IsSuccessful = result.IsSuccessful,
                    TransactionId = result.TransactionId,
                    PaymentUrl = result.PaymentUrl,
                    Status = result.Status.ToString(),
                    ErrorMessage = result.ErrorMessage,
                    Timestamp = result.Timestamp
                });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception during payment processing");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("refund")]
        public async Task<ActionResult<RefundResponseDto>> RefundPayment([FromBody] RefundRequestDto request)
        {
            try
            {
                var command = new RefundBookingPaymentCommand
                {
                    BookingId = request.BookingId,
                    RefundAmount = request.RefundAmount,
                    Reason = request.Reason
                };

                var result = await _mediator.Send(command);

                return Ok(new RefundResponseDto
                {
                    IsSuccessful = result.IsSuccessful,
                    RefundId = result.RefundId,
                    RefundedAmount = result.RefundedAmount,
                    ErrorMessage = result.ErrorMessage,
                    Timestamp = result.Timestamp
                });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception during refund processing");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("status/{bookingId}")]
        public async Task<ActionResult<PaymentStatusDto>> GetPaymentStatus(Guid bookingId)
        {
            try
            {
                var query = new GetPaymentStatusQuery { BookingId = bookingId };
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception getting payment status");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("verify/{bookingId}")]
        public async Task<ActionResult<PaymentResponseDto>> VerifyPayment(Guid bookingId)
        {
            try
            {
                var command = new VerifyPaymentCommand { BookingId = bookingId };
                var result = await _mediator.Send(command);

                return Ok(new PaymentResponseDto
                {
                    IsSuccessful = result.IsSuccessful,
                    TransactionId = result.TransactionId,
                    Status = result.Status.ToString(),
                    ErrorMessage = result.ErrorMessage,
                    Timestamp = result.Timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        [HttpGet("success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string tap_id, [FromQuery] string booking_id)
        {
            try
            {
                if (Guid.TryParse(booking_id, out var bookingGuid))
                {
                    // Verify the payment status
                    var command = new VerifyPaymentCommand { BookingId = bookingGuid };
                    await _mediator.Send(command);
                }

                // Return success page or redirect to your app
                return Ok(new { message = "Payment successful", bookingId = booking_id, tapId = tap_id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment success callback");
                return StatusCode(500, new { message = "Error processing payment confirmation" });
            }
        }

        [HttpGet("cancel")]
        public IActionResult PaymentCancel([FromQuery] string booking_id)
        {
            // Handle payment cancellation
            return Ok(new { message = "Payment cancelled", bookingId = booking_id });
        }

        [HttpGet("fail")]
        public IActionResult PaymentFail([FromQuery] string booking_id, [FromQuery] string error)
        {
            // Handle payment failure
            return Ok(new { message = "Payment failed", bookingId = booking_id, error = error });
        }

        [HttpPost("tap")]
        public async Task<IActionResult> HandleTapWebhook([FromBody] JsonElement payload)
        {
            try
            {
                // Verify webhook signature (implement based on Tap's documentation)
                // var signature = Request.Headers["tap-signature"].FirstOrDefault();
                // if (!VerifyWebhookSignature(payload, signature))
                //     return Unauthorized();

                var eventType = payload.GetProperty("event").GetString();
                var chargeId = payload.GetProperty("data").GetProperty("id").GetString();

                _logger.LogInformation($"Received Tap webhook: {eventType} for charge {chargeId}");

                // Handle different webhook events
                switch (eventType)
                {
                    case "payment.success":
                        await HandlePaymentSuccess(payload);
                        break;
                    case "payment.failed":
                        await HandlePaymentFailed(payload);
                        break;
                    case "refund.success":
                        await HandleRefundSuccess(payload);
                        break;
                        // Add more event types as needed
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Tap webhook");
                return StatusCode(500);
            }
        }

        private async Task HandlePaymentSuccess(JsonElement payload)
        {
            // Extract booking ID from metadata and update payment status
            var chargeData = payload.GetProperty("data");
            var metadata = chargeData.GetProperty("metadata");

            if (metadata.TryGetProperty("booking_id", out var bookingIdProp))
            {
                var bookingId = Guid.Parse(bookingIdProp.GetString());
                // Update payment status to completed
                // Implement command to handle webhook payment success
            }
        }

        private async Task HandlePaymentFailed(JsonElement payload)
        {
            // Similar to success but mark as failed
            var chargeData = payload.GetProperty("data");
            var metadata = chargeData.GetProperty("metadata");

            if (metadata.TryGetProperty("booking_id", out var bookingIdProp))
            {
                var bookingId = Guid.Parse(bookingIdProp.GetString());
                // Update payment status to failed
                // Implement command to handle webhook payment failure
            }
        }

        private async Task HandleRefundSuccess(JsonElement payload)
        {
            // Handle refund webhook
            // Update refund status in database
        }
    }
}

