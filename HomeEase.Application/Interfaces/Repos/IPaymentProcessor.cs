using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Repos
{
    public interface IPaymentProcessor
    {
        Task<PaymentResult> ProcessPaymentAsync(Guid bookingId, decimal amount, string currency, string paymentMethod, string transactionId = null);
    }
}
