using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.DTOs
{
    public class PaymentInfoDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public CustomerInfoDto Customer { get; set; }
    }

    public class ProcessPaymentRequest
    {
        public Guid BookingId { get; set; }
        public PaymentInfoDto PaymentInfo { get; set; }
    }

    public class CustomerInfoDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneCountryCode { get; set; } = "966";
    }

    public class PaymentResponseDto
    {
        public bool IsSuccessful { get; set; }
        public string TransactionId { get; set; }
        public string PaymentUrl { get; set; } // For redirect to payment page
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PaymentStatusDto
    {
        public Guid BookingId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TransactionId { get; set; }
        public string TapChargeId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal? RefundedAmount { get; set; }
        public DateTime? RefundedAt { get; set; }
    }

    public class RefundRequestDto
    {
        public Guid BookingId { get; set; }
        public decimal? RefundAmount { get; set; } // null for full refund
        public string Reason { get; set; } = "requested_by_customer";
    }

    public class RefundResponseDto
    {
        public bool IsSuccessful { get; set; }
        public string RefundId { get; set; }
        public decimal RefundedAmount { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
