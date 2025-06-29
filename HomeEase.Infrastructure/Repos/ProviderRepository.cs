using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Repositories;
using HomeEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;


namespace HomeEase.Infrastructure.Repos;

public class ProviderRepository(AppDbContext _dbContext) : IProviderRepository
{
    public async Task<Provider> GetByIdAsync(Guid id)
    {
        return await _dbContext.Providers
            .Include(p => p.Address)
            .Include(p => p.Images)
            .Include(p => p.Services).ThenInclude(s => s.BasePlatformService)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> CheckAvailabilityAsync(Guid providerId, DateTime appointmentTime, int durationMinutes)
    {
        // Step 1: Validate provider exists
        var providerExists = await _dbContext.Providers
            .AnyAsync(p => p.Id == providerId);

        if (!providerExists)
            return false;

        // Step 2: Determine appointment time window
        var dayOfWeek = appointmentTime.DayOfWeek;
        var timeOfDay = appointmentTime.TimeOfDay;
        var endTimeOfDay = timeOfDay.Add(TimeSpan.FromMinutes(durationMinutes));

        // Step 3: Check for a matching availability slot
        var hasAvailabilitySlot = await _dbContext.AvailabilitySlots
            .AnyAsync(slot =>
                slot.ProviderId == providerId &&
                (
                    // Recurring slot (e.g., every Monday 10:00–12:00)
                    slot.IsRecurring && slot.DayOfWeek == dayOfWeek ||

                    // One-time specific date slot
                    !slot.IsRecurring &&
                     slot.SpecificDate.HasValue &&
                     slot.SpecificDate.Value.Date == appointmentTime.Date
                ) &&
                slot.StartTime <= timeOfDay &&
                slot.EndTime >= endTimeOfDay
            );

        if (!hasAvailabilitySlot)
            return false;

        // Step 4: Check for conflicting bookings at the same time
        var appointmentEndTime = appointmentTime.AddMinutes(durationMinutes);

        var hasConflict = await _dbContext.Bookings
            .AnyAsync(booking =>
                booking.ProviderId == providerId &&
                booking.Status != BookingStatus.Cancelled &&
                booking.AppointmentDateTime < appointmentEndTime &&
                booking.AppointmentDateTime.AddMinutes(booking.DurationMinutes) > appointmentTime
            );

        // Return true only if no conflict exists
        return !hasConflict;
    }

    public async Task<Provider> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbContext.Providers
            .Include(p => p.Address)
            .Include(p => p.User) // <-- Include User
            .Include(p => p.Schedule)
                .ThenInclude(s => s.RegularHours)
            .Include(p => p.Schedule)
                .ThenInclude(s => s.SpecialDates)
            .Include(p => p.Schedule)
                .ThenInclude(s => s.AvailableSlots)
            .Include(p => p.Services).ThenInclude(s => s.BasePlatformService)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Provider> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.Providers
            .Include(p => p.Images)
            .Include(p => p.Services).ThenInclude(s => s.BasePlatformService)
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
            .Include(p => p.Services).ThenInclude(s => s.BasePlatformService)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<(IEnumerable<Provider> Provider, int totalCount)> GetAllProvidersAsync(
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
    )
    {
        var query = _dbContext.Providers
            .Include(p => p.Address)
            .Include(p => p.User)
            .Include(p => p.Services).ThenInclude(s => s.BasePlatformService)
            .Include(p => p.Images)
            .Where(p => p.User.Role == UserRole.Provider)
            .Where(p => p.Services.Any())
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string lowerSearch = searchTerm.ToLower();

            query = query.Where(p =>
                p.BusinessName.ToLower().Contains(lowerSearch) ||
                p.Email.ToLower().Contains(lowerSearch) ||
                p.User.FirstName.ToLower().Contains(lowerSearch) ||
                p.User.LastName.ToLower().Contains(lowerSearch) ||
                p.User.Email.ToLower().Contains(lowerSearch) ||
                p.User.PhoneNumber.ToLower().Contains(lowerSearch)
            );
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p =>
                p.Services.All(s => s.Price >= minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p =>
                p.Services.All(s => s.Price <= maxPrice.Value));
        }


        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(p => p.Address.City.ToLower().Contains(city.ToLower()));
        }

        if (isHomeServiceAvailable.HasValue)
        {
            query = query.Where(p => p.Services.Any(s => s.HomePrice > 0));
        }

        if (isCenterServiceAvailable.HasValue)
        {
            query = query.Where(p => p.Services.Any(s => s.Price > 0));
        }
      
        if (minAverageServiceRating.HasValue)
        {
            query = query.Where(p => p.Services.Any() && p.Services.Average(s => s.Rating) >= minAverageServiceRating.Value);
        }


        if (basePlatformServiceIds != null && basePlatformServiceIds.Any())
        {
            query = query.Where(p => p.Services.Any(s => basePlatformServiceIds.Contains(s.BasePlatformService.Id)));
        }

        query = sortBy.ToLower() switch
        {
            "createdat" => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "updatedat" => sortDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt),
            "rating" => sortDescending ? query.OrderByDescending(p => p.Services.Any() ? p.Services.Average(s => s.Rating) : 0) : query.OrderBy(p => p.Services.Any() ? p.Services.Average(s => s.Rating) : 0),
            _ => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var paginatedProviders = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (paginatedProviders, totalCount);
    }





    public async Task<List<Provider>> SearchProvidersAsync(
        double? latitude,
        double? longitude,
        double? maxDistance,
        decimal? minRating,
        string city,
        string state,
        int pageNumber,
        int pageSize)
    {
        IQueryable<Provider> query = _dbContext.Providers
            .Include(p => p.Address)
            .Include(p => p.Services)
            .Include(p => p.Images)
            .Include(p => p.Services).ThenInclude(s => s.BasePlatformService)
            .Where(p => p.Status != ProviderStatus.Suspended);


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
