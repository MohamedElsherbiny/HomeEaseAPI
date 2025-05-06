using Microsoft.EntityFrameworkCore;
using Massage.Application.Interfaces;
using Massage.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Massage.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SpecialDate> SpecialDates { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<WorkingHours> WorkingHours { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ProviderSchedule> ProviderSchedules { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<PaymentInfo> PaymentInfos { get; set; }
        public DbSet<AvailabilitySlots> AvailabilitySlots { get; set; }
 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}



