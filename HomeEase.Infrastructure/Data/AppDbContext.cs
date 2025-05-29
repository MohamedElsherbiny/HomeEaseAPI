using Microsoft.EntityFrameworkCore;
using HomeEase.Application.Interfaces;
using HomeEase.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace HomeEase.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options), IAppDbContext
{
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
    public DbSet<BasePlatformService> BasePlatformService { get; set; }
    //public DbSet<PaymentResult> PaymentResult { get; set; }
    //public DbSet<PaymentSettings> PaymentSettings { get; set; }
    //public DbSet<NotificationSettings> NotificationSettings { get; set; }
    //public DbSet<EmailModel> EmailModel { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}



