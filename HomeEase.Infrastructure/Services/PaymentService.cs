using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using Microsoft.Extensions.Options;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentSettings _paymentSettings;

        public PaymentService(IOptions<PaymentSettings> paymentSettings)
        {
            _paymentSettings = paymentSettings.Value;
            StripeConfiguration.ApiKey = _paymentSettings.ApiKey;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentInfo paymentInfo, string paymentToken)
        {
            try
            {
                var options = new ChargeCreateOptions
                {
                    Amount = (long)(paymentInfo.Amount * 100), // Convert to cents
                    Currency = paymentInfo.Currency,
                    Source = paymentToken,
                    Description = $"Payment for Booking {paymentInfo.BookingId}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", paymentInfo.BookingId.ToString() }
                    }
                };

                var service = new ChargeService();
                var charge = await service.CreateAsync(options);

                return new PaymentResult
                {
                    IsSuccessful = charge.Status == "succeeded",
                    TransactionId = charge.Id,
                    ErrorMessage = charge.Status != "succeeded" ? charge.FailureMessage : null,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (StripeException ex)
            {
                return new PaymentResult
                {
                    IsSuccessful = false,
                    TransactionId = null,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<PaymentResult> RefundPaymentAsync(PaymentInfo paymentInfo)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    Charge = paymentInfo.TransactionId,
                    Amount = (long)(paymentInfo.Amount * 100) // Convert to cents
                };

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                return new PaymentResult
                {
                    IsSuccessful = refund.Status == "succeeded",
                    TransactionId = refund.Id,
                    ErrorMessage = refund.Status != "succeeded" ? refund.FailureReason : null,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (StripeException ex)
            {
                return new PaymentResult
                {
                    IsSuccessful = false,
                    TransactionId = null,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
