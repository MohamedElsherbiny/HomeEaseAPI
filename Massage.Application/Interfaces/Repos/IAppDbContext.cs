using Microsoft.EntityFrameworkCore;
using Massage.Domain.Entities;

namespace Massage.Application.Interfaces
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
        DbSet<Address> Addresses { get; set; }
        DbSet<PaymentInfo> PaymentInfos { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
