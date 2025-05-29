using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentInfo paymentInfo, string paymentToken);
        Task<PaymentResult> RefundPaymentAsync(PaymentInfo paymentInfo);
    }
}
