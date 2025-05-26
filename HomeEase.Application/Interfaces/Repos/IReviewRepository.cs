using HomeEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.Interfaces.Repos
{
    public interface IReviewRepository
    {
        Task<Review> GetByIdAsync(Guid id);
        Task<IEnumerable<Review>> GetByProviderIdAsync(Guid providerId, int pageNumber, int pageSize);
        Task<IEnumerable<Review>> GetAllAsync(int pageNumber, int pageSize);
        Task AddAsync(Review review);
        void Update(Review review);
        void Delete(Review review);
        Task<Review> GetByBookingIdAsync(Guid bookingId);
    }
}
