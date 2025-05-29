using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeEase.Infrastructure.Repos
{
    public class PaymentInfoRepository : IPaymentInfoRepository
    {
        private readonly IAppDbContext _context;

        public PaymentInfoRepository(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentInfo> GetByIdAsync(Guid id)
        {
            return await _context.PaymentInfos.FindAsync(id);
        }

        public async Task<IEnumerable<PaymentInfo>> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.PaymentInfos
                .Where(p => p.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentInfo>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _context.PaymentInfos
                .OrderByDescending(p => p.ProcessedAt ?? DateTime.UtcNow)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(PaymentInfo paymentInfo)
        {
            await _context.PaymentInfos.AddAsync(paymentInfo);
        }

        public void Update(PaymentInfo paymentInfo)
        {
            _context.PaymentInfos.Update(paymentInfo);
        }

        public void Delete(PaymentInfo paymentInfo)
        {
            _context.PaymentInfos.Remove(paymentInfo);
        }
    }
}
