using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.DTOs
{
    public class PaymentInfoDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class CreatePaymentDto
    {
        public Guid BookingId { get; set; }
        public string PaymentMethod { get; set; } // e.g., "card", "paypal"
        public string PaymentToken { get; set; } // e.g., Stripe token
    }

    public class UpdatePaymentDto
    {
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class PaymentResultDto
    {
        public bool IsSuccessful { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
