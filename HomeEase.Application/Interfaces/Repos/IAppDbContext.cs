using Microsoft.EntityFrameworkCore;
using HomeEase.Domain.Entities;

namespace HomeEase.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Provider> Providers { get; set; }
        DbSet<Service> Services { get; set; }
        DbSet<Location> Locations { get; set; }
        DbSet<Schedule> Schedules { get; set; }
        DbSet<Booking> Bookings { get; set; }
        DbSet<Review> Reviews { get; set; }
        DbSet<SpecialDate> SpecialDates { get; set; }
        DbSet<TimeSlot> TimeSlots { get; set; }
        DbSet<WorkingHours> WorkingHours { get; set; }
        DbSet<Address> Addresses { get; set; }
        DbSet<ProviderSchedule> ProviderSchedules { get; set; }
        DbSet<PaymentInfo> PaymentInfos { get; set; }
        DbSet<AvailabilitySlots> AvailabilitySlots { get; set; }
        DbSet<BasePlatformService> BasePlatformService { get; set; }
        DbSet<ProviderImage> ProviderImages { get; set; }
        DbSet<UserServiceLike> UserServiceLikes { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
