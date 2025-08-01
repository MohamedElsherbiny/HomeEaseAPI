using HomeEase.Application.Interfaces.Repos;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static System.Net.WebRequestMethods;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace HomeEase.Infrastructure.Services;

public class TapPaymentProcessor : IPaymentProcessor
{
    private readonly HttpClient _httpClient;
    private readonly TapGatewaySettings _settings;
    private readonly ILogger<TapPaymentProcessor> _logger;

    public TapPaymentProcessor(
        HttpClient httpClient,
        IOptions<TapGatewaySettings> settings,
        ILogger<TapPaymentProcessor> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SecretApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<PaymentResult> ProcessPaymentAsync(
        Guid bookingId,
        decimal amount,
        string currency,
        string paymentMethod,
        CustomerInfo customer)
    {
        try
        {
            _logger.LogInformation($"Processing Tap payment for booking {bookingId}. Amount: {amount} {currency}");

            var chargeRequest = new TapChargeRequest
            {
                amount = amount,
                currency = currency.ToUpper(),
                customer_initiated = true, // ✅ مفقود في الكود الأصلي
                threeDSecure = true,
                save_card = false, // ✅ مفقود في الكود الأصلي
                description = $"Massage booking payment - {bookingId}",
                statement_descriptor = "HomeEase Massage",

                // ✅ إضافة metadata
                metadata = new Dictionary<string, string>
            {
                { "booking_id", bookingId.ToString() },
                { "service", "massage" }
            },

                // ✅ إضافة receipt settings
                receipt = new TapReceipt
                {
                    email = false,
                    sms = false
                },

                // ✅ إضافة reference
                reference = new TapReference
                {
                    transaction = $"txn_{bookingId}",
                    order = $"ord_{bookingId}"
                },

                customer = new TapCustomer
                {
                    first_name = customer.FirstName,
                    last_name = customer.LastName,
                    email = customer.Email,
                    phone = new TapPhone
                    {
                        country_code = customer.PhoneCountryCode ?? "966",
                        number = customer.PhoneNumber?.Replace("+", "").Replace(" ", "")
                    }
                },
                source = new TapSource { id = "src_card" }, // For new card payments
                //redirect = new TapRedirect { url = _settings.RedirectUrl },
                redirect = new TapRedirect { url = "https://yoursite.com/payment/success" },
                post = new TapPost { url = "https://yoursite.com/api/paymentwebhook/tap" }
            };

            var json = JsonSerializer.Serialize(chargeRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogInformation($"Sending request to Tap: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/charges", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Tap response: Status={response.StatusCode}, Content={responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var chargeResponse = JsonSerializer.Deserialize<TapChargeResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var result = new PaymentResult
                {
                    IsSuccessful = chargeResponse.status == "CAPTURED",
                    TransactionId = chargeResponse.id, // ✅ استخدم chargeResponse.id بدلاً من transaction.url
                    TapChargeId = chargeResponse.id,
                    PaymentUrl = chargeResponse.redirect?.url, // ✅ تحديث المسار
                    Status = MapTapStatusToPaymentStatus(chargeResponse.status),
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation($"Tap payment processed. Status: {chargeResponse.status}, Charge ID: {chargeResponse.id}");
                return result;
            }
            else
            {
                _logger.LogError($"Tap payment failed. Status: {response.StatusCode}, Response: {responseContent}");

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var errorMessage = errorResponse.TryGetProperty("message", out var msgProp)
                        ? msgProp.GetString()
                        : "Payment processing failed";

                    return new PaymentResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = errorMessage,
                        Status = PaymentStatus.Failed,
                        Timestamp = DateTime.UtcNow
                    };
                }
                catch
                {
                    return new PaymentResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = "Payment processing failed - invalid response from gateway",
                        Status = PaymentStatus.Failed,
                        Timestamp = DateTime.UtcNow
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception during payment processing for booking {bookingId}");
            return new PaymentResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                Status = PaymentStatus.Failed,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<RefundResult> RefundPaymentAsync(string chargeId, decimal amount, string currency, string reason = "requested_by_customer")
    {
        try
        {
            _logger.LogInformation($"Processing refund for charge {chargeId}. Amount: {amount} {currency}");

            var refundRequest = new TapRefundRequest
            {
                charge_id = chargeId,
                amount = amount,
                currency = currency,
                description = "Massage booking refund",
                reason = reason,
                metadata = new { refund_date = DateTime.UtcNow },
                post = new TapPost { url = _settings.PostUrl }
            };

            var json = JsonSerializer.Serialize(refundRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/refunds", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var refundResponse = JsonSerializer.Deserialize<TapRefundResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation($"Refund processed successfully. Refund ID: {refundResponse.id}");

                return new RefundResult
                {
                    IsSuccessful = true,
                    RefundId = refundResponse.id,
                    RefundedAmount = refundResponse.amount,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                _logger.LogError($"Refund failed. Status: {response.StatusCode}, Response: {responseContent}");
                return new RefundResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Refund processing failed",
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception during refund processing for charge {chargeId}");
            return new RefundResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<PaymentResult> GetPaymentStatusAsync(string chargeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/charges/{chargeId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var chargeResponse = JsonSerializer.Deserialize<TapChargeResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return new PaymentResult
                {
                    IsSuccessful = chargeResponse.status == "CAPTURED",
                    TransactionId = chargeResponse.transaction?.url ?? chargeResponse.id,
                    TapChargeId = chargeResponse.id,
                    Status = MapTapStatusToPaymentStatus(chargeResponse.status),
                    Timestamp = DateTime.UtcNow
                };
            }

            return new PaymentResult
            {
                IsSuccessful = false,
                ErrorMessage = "Failed to retrieve payment status",
                Status = PaymentStatus.Failed,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception retrieving payment status for charge {chargeId}");
            return new PaymentResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                Status = PaymentStatus.Failed,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private static PaymentStatus MapTapStatusToPaymentStatus(string tapStatus)
    {
        return tapStatus?.ToUpper() switch
        {
            "INITIATED" => PaymentStatus.Pending,
            "ABANDONED" => PaymentStatus.Cancelled,
            "CANCELLED" => PaymentStatus.Cancelled,
            "FAILED" => PaymentStatus.Failed,
            "DECLINED" => PaymentStatus.Failed,
            "RESTRICTED" => PaymentStatus.Failed,
            "CAPTURED" => PaymentStatus.Completed,
            "VOID" => PaymentStatus.Cancelled,
            "TIMEDOUT" => PaymentStatus.Failed,
            "UNKNOWN" => PaymentStatus.Failed,
            _ => PaymentStatus.Failed
        };
    }
}



public class PaymentStatusMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentStatusMonitorService> _logger;

    public PaymentStatusMonitorService(
        IServiceProvider serviceProvider,
        ILogger<PaymentStatusMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var paymentProcessor = scope.ServiceProvider.GetRequiredService<IPaymentProcessor>();

                // Get pending payments older than 10 minutes
                var pendingPayments = await bookingRepository.GetPendingPaymentsAsync(TimeSpan.FromMinutes(10));

                foreach (var payment in pendingPayments)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(payment.TapChargeId))
                        {
                            var status = await paymentProcessor.GetPaymentStatusAsync(payment.TapChargeId);

                            if (payment.Status != status.Status.ToString())
                            {
                                payment.Status = status.Status.ToString();
                                if (status.IsSuccessful)
                                {
                                    payment.ProcessedAt = DateTime.UtcNow;
                                }

                                await bookingRepository.UpdatePaymentAsync(payment);
                                _logger.LogInformation($"Updated payment status for booking {payment.BookingId}: {status.Status}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error checking payment status for booking {payment.BookingId}");
                    }
                }

                // Wait 5 minutes before next check
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in payment status monitor service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}