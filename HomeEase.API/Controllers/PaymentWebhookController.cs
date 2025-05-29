using HomeEase.Application.Commands.PaymentCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Queries.PaymentQueries;
using HomeEase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;


namespace HomeEase.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly PaymentSettings _paymentSettings;

        public PaymentWebhookController(IMediator mediator, IOptions<PaymentSettings> paymentSettings)
        {
            _mediator = mediator;
            _paymentSettings = paymentSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _paymentSettings.WebhookSecret);

                if (stripeEvent.Type == "charge.succeeded")
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    if (charge == null || !charge.Metadata.ContainsKey("BookingId"))
                        return BadRequest("Invalid charge or missing BookingId metadata.");

                    var bookingId = Guid.Parse(charge.Metadata["BookingId"]);
                    var payments = await _mediator.Send(new GetPaymentsByBookingIdQuery { BookingId = bookingId });
                    var paymentInfo = payments.FirstOrDefault(p => p.TransactionId == charge.Id);
                    if (paymentInfo != null)
                    {
                        await _mediator.Send(new UpdatePaymentCommand
                        {
                            Id = paymentInfo.Id,
                            PaymentDto = new UpdatePaymentDto
                            {
                                Status = "Completed",
                                TransactionId = charge.Id,
                                ProcessedAt = DateTime.UtcNow
                            }
                        });
                    }
                }
                else if (stripeEvent.Type == "charge.failed")
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    if (charge == null || !charge.Metadata.ContainsKey("BookingId"))
                        return BadRequest("Invalid charge or missing BookingId metadata.");

                    var bookingId = Guid.Parse(charge.Metadata["BookingId"]);
                    var payments = await _mediator.Send(new GetPaymentsByBookingIdQuery { BookingId = bookingId });
                    var paymentInfo = payments.FirstOrDefault(p => p.TransactionId == charge.Id);
                    if (paymentInfo != null)
                    {
                        await _mediator.Send(new UpdatePaymentCommand
                        {
                            Id = paymentInfo.Id,
                            PaymentDto = new UpdatePaymentDto
                            {
                                Status = "Failed",
                                TransactionId = charge.Id,
                                ProcessedAt = null
                            }
                        });
                    }
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest($"Webhook error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }
    }
}