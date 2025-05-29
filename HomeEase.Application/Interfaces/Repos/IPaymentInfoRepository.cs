using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Repos
{
    public interface IPaymentInfoRepository
    {
        Task<PaymentInfo> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentInfo>> GetByBookingIdAsync(Guid bookingId);
        Task<IEnumerable<PaymentInfo>> GetAllAsync(int pageNumber, int pageSize);
        Task AddAsync(PaymentInfo paymentInfo);
        void Update(PaymentInfo paymentInfo);
        void Delete(PaymentInfo paymentInfo);
    }
}
