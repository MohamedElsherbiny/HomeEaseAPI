namespace HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;


public class PaymentInfo
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string Status { get; set; } // Pending, Processing, Completed, Failed, Cancelled, Refunded
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR"; 
    public string PaymentMethod { get; set; }
    public string TransactionId { get; set; }
    public string TapChargeId { get; set; } // Tap Gateway charge ID
    public string TapPaymentId { get; set; } // Tap Gateway payment ID
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundedAmount { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string PaymentUrl { get; set; } 
    public string WebhookData { get; set; }
    public Booking Booking { get; set; }
}

public class PaymentResult
{
    public bool IsSuccessful { get; set; }
    public string TransactionId { get; set; }
    public string TapChargeId { get; set; }
    public string PaymentUrl { get; set; } // For redirect payments
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public PaymentStatus Status { get; set; }
}

public class RefundResult
{
    public bool IsSuccessful { get; set; }
    public string RefundId { get; set; }
    public decimal RefundedAmount { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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



public class TapGatewaySettings
{
    public string SecretApiKey { get; set; }
    public string PublishableKey { get; set; }
    public string WebhookSecret { get; set; }
    public string BaseUrl { get; set; } = "https://api.tap.company/v2";
    public bool UseSandbox { get; set; }
    public string RedirectUrl { get; set; }
    public string PostUrl { get; set; }
}


public class TapChargeRequest
{
    public decimal amount { get; set; }
    public string currency { get; set; }
    public bool threeDSecure { get; set; } = true;
    public bool save_card { get; set; } = false;
    public string description { get; set; }
    public string statement_descriptor { get; set; }
    public TapCustomer customer { get; set; }
    public TapSource source { get; set; }
    public TapPost post { get; set; }
    public TapRedirect redirect { get; set; }
}

public class TapCustomer
{
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string email { get; set; }
    public TapPhone phone { get; set; }
}

public class TapPhone
{
    public string country_code { get; set; }
    public string number { get; set; }
}

public class TapSource
{
    public string id { get; set; } // For saved cards or "src_card" for new cards
}

public class TapPost
{
    public string url { get; set; }
}

public class TapRedirect
{
    public string url { get; set; }
}

public class TapChargeResponse
{
    public string id { get; set; }
    public bool live_mode { get; set; }
    public string api_version { get; set; }
    public string method { get; set; }
    public string status { get; set; }
    public decimal amount { get; set; }
    public string currency { get; set; }
    public bool threeDSecure { get; set; }
    public bool card_threeDSecure { get; set; }
    public bool save_card { get; set; }
    public string description { get; set; }
    public string statement_descriptor { get; set; }
    public TapTransaction transaction { get; set; }
    public TapCustomer customer { get; set; }
    public TapSource source { get; set; }
    public TapRedirect redirect { get; set; }
    public TapPost post { get; set; }
    public string auto_reversed { get; set; }
    public string gateway_response { get; set; }
    public DateTime created { get; set; }
}

public class TapTransaction
{
    public string timezone { get; set; }
    public DateTime created { get; set; }
    public string url { get; set; }
    public string expiry { get; set; }
    public bool asynchronous { get; set; }
    public decimal amount { get; set; }
    public string currency { get; set; }
}

public class TapRefundRequest
{
    public string charge_id { get; set; }
    public decimal amount { get; set; }
    public string currency { get; set; }
    public string description { get; set; }
    public string reason { get; set; }
    public object metadata { get; set; }
    public TapPost post { get; set; }
}

public class TapRefundResponse
{
    public string id { get; set; }
    public bool live_mode { get; set; }
    public string api_version { get; set; }
    public string method { get; set; }
    public string status { get; set; }
    public decimal amount { get; set; }
    public string currency { get; set; }
    public string charge_id { get; set; }
    public string description { get; set; }
    public string reason { get; set; }
    public DateTime created { get; set; }
}

public class CustomerInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCountryCode { get; set; } = "966"; // Saudi Arabia default
}
 