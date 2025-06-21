using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeEase.Domain.Entities;

namespace HomeEase.Domain.Repositories
{
    public interface IProviderRepository
    {
        Task<Provider> GetByIdAsync(Guid id);
        Task<bool> CheckAvailabilityAsync(Guid providerId, DateTime appointmentTime, int durationMinutes);
        Task<Provider> GetByIdWithDetailsAsync(Guid id);
        Task<Provider> GetByUserIdAsync(Guid userId);
        Task<Provider> GetByUserIdWithDetailsAsync(Guid userId);

        Task<(IEnumerable<Provider> Provider, int totalCount)> GetAllProvidersAsync(
              int pageNumber,
              int pageSize,
              string? searchTerm,
              string sortBy,
              bool sortDescending,
              decimal? minPrice,
              decimal? maxPrice,
              string? city,
              bool? isHomeServiceAvailable,
              bool? isCenterServiceAvailable,
              decimal? minAverageServiceRating,
              List<Guid>? basePlatformServiceIds
  );

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

