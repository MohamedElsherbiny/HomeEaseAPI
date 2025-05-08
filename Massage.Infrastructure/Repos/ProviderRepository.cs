using Massage.Domain.Entities;
using Massage.Domain.Enums;
using Massage.Domain.Repositories;
using Massage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;


namespace Massage.Infrastructure.Services
{
    public class ProviderRepository : IProviderRepository
    {
        private readonly AppDbContext _dbContext;

        public ProviderRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Provider> GetByIdAsync(Guid id)
        {
            return await _dbContext.Providers
                .Include(p => p.Address)
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task<bool> CheckAvailabilityAsync(Guid providerId, DateTime appointmentTime, int durationMinutes)
        {
            // Check if the provider exists
            var provider = await _dbContext.Providers
                .AnyAsync(p => p.Id == providerId);

            if (!provider)
            {
                return false;
            }

            var dayOfWeek = appointmentTime.DayOfWeek;
            var timeOfDay = appointmentTime.TimeOfDay;
            var endTime = timeOfDay.Add(TimeSpan.FromMinutes(durationMinutes));

            // Check if there's an availability slot for this day and time
            var hasAvailabilitySlot = await _dbContext.AvailabilitySlots
                .AnyAsync(a => a.ProviderId == providerId &&
                               ((a.IsRecurring && a.DayOfWeek == dayOfWeek) ||
                                (!a.IsRecurring && a.SpecificDate.HasValue && a.SpecificDate.Value.Date == appointmentTime.Date)) &&
                               a.StartTime <= timeOfDay && a.EndTime >= endTime);

            if (!hasAvailabilitySlot)
            {
                return false;
            }

            // Check if there are any conflicting bookings
            var appointmentEndTime = appointmentTime.AddMinutes(durationMinutes);

            var hasConflictingBooking = await _dbContext.Bookings
                .AnyAsync(b => b.ProviderId == providerId &&
                               b.Status != BookingStatus.Cancelled &&
                               b.AppointmentDateTime < appointmentEndTime &&
                               b.AppointmentDateTime.AddMinutes(b.DurationMinutes) > appointmentTime);

            return !hasConflictingBooking;
        }
        public async Task<Provider> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbContext.Providers
                .Include(p => p.Address)
                .Include(p => p.Schedule)
                    .ThenInclude(s => s.RegularHours)
                .Include(p => p.Schedule)
                    .ThenInclude(s => s.SpecialDates)
                .Include(p => p.Schedule)
                    .ThenInclude(s => s.AvailableSlots)
                .Include(p => p.Services)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Provider> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Providers
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Provider> GetByUserIdWithDetailsAsync(Guid userId)
        {
            return await _dbContext.Providers
                .Include(p => p.Address)
                .Include(p => p.Schedule)
                    .ThenInclude(s => s.RegularHours)
                .Include(p => p.Schedule)
                    .ThenInclude(s => s.SpecialDates)
                .Include(p => p.Schedule)
                    .ThenInclude(s => s.AvailableSlots)
                .Include(p => p.Services)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<List<Provider>> GetAllAsync()
        {
            return await _dbContext.Providers
                .Include(p => p.Address)
                .ToListAsync();
        }


        public async Task<List<Provider>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            return await _dbContext.Providers
                .Include(p => p.Address)
                .OrderByDescending(p => p.Rating)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        public async Task<List<Provider>> SearchProvidersAsync(
            double? latitude,
            double? longitude,
            double? maxDistance,
            string[] serviceTypes,
            decimal? minRating,
            string city,
            string state,
            int pageNumber,
            int pageSize)
        {
            IQueryable<Provider> query = _dbContext.Providers
                .Include(p => p.Address)
                .Include(p => p.Services)
                .Where(p => p.Status != ProviderStatus.Suspended);

            // Filter by service types
            if (serviceTypes != null && serviceTypes.Length > 0)
            {
                query = query.Where(p => p.ServiceTypes.Any(st => serviceTypes.Contains(st)));
            }

            // Filter by minimum rating
            if (minRating.HasValue)
            {
                query = query.Where(p => p.Rating >= minRating.Value);
            }

            // Filter by location (city and state)
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(p => p.Address.City.ToLower() == city.ToLower());
            }

            if (!string.IsNullOrEmpty(state))
            {
                query = query.Where(p => p.Address.State.ToLower() == state.ToLower());
            }

            // Sort by distance if coordinates are provided
            if (latitude.HasValue && longitude.HasValue)
            {
                var userLocation = new Point(longitude.Value, latitude.Value) { SRID = 4326 };

                // Convert to a list to perform in-memory distance calculation
                var providers = await query.ToListAsync();

                // Calculate distance for each provider
                var providersWithDistance = providers
                    .Where(p => p.Address.Latitude.HasValue && p.Address.Longitude.HasValue)
                    .Select(p =>
                    {
                        // Calculate distance in kilometers
                        var providerLocation = new Point((double)p.Address.Longitude.Value, (double)p.Address.Latitude.Value) { SRID = 4326 };
                        var distanceInMeters = userLocation.Distance(providerLocation);
                        var distanceInKm = distanceInMeters / 1000;

                        // Create an anonymous type with provider and distance
                        return new { Provider = p, Distance = distanceInKm };
                    })
                    .ToList();

                // Filter by max distance if provided
                if (maxDistance.HasValue)
                {
                    providersWithDistance = providersWithDistance
                        .Where(pwd => pwd.Distance <= maxDistance.Value)
                        .ToList();
                }

                // Order by distance and apply pagination
                return providersWithDistance
                    .OrderBy(pwd => pwd.Distance)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(pwd => pwd.Provider)
                    .ToList();
            }

            // If no coordinates provided, sort by rating and apply pagination
            return await query
                .OrderByDescending(p => p.Rating)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(Provider provider)
        {
            await _dbContext.Providers.AddAsync(provider);
        }

        public void Update(Provider provider)
        {
            _dbContext.Providers.Update(provider);
        }

        public void Delete(Provider provider)
        {
            _dbContext.Providers.Remove(provider);
        }
    }
}
