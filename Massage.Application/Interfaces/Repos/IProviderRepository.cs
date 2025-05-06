using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Massage.Domain.Entities;

namespace Massage.Domain.Repositories
{
    public interface IProviderRepository
    {
        Task<Provider> GetByIdAsync(Guid id);
        Task<bool> CheckAvailabilityAsync(Guid providerId, DateTime appointmentTime, int durationMinutes);
        Task<Provider> GetByIdWithDetailsAsync(Guid id);
        Task<Provider> GetByUserIdAsync(Guid userId);
        Task<Provider> GetByUserIdWithDetailsAsync(Guid userId);
        Task<List<Provider>> GetAllAsync();
        Task<List<Provider>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
        Task<List<Provider>> SearchProvidersAsync(
            double? latitude,
            double? longitude,
            double? maxDistance,
            string[] serviceTypes,
            decimal? minRating,
            string city,
            string state,
            int pageNumber,
            int pageSize);
        Task AddAsync(Provider provider);
        void Update(Provider provider);
        void Delete(Provider provider);
    }
}

