using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Domain.Entities
{
    public class PaymentInfo
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

    public class PaymentResult
    {
        public bool IsSuccessful { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }


    public class PaymentSettings
    {
        public string ApiKey { get; set; }
        public string WebhookSecret { get; set; }
        public bool UseSandbox { get; set; }
    }
    public class NotificationSettings
    {
        public string ApiKey { get; set; }
        public string ApiEndpoint { get; set; }
        public string FromEmail { get; set; }
    }

    public class EmailModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
