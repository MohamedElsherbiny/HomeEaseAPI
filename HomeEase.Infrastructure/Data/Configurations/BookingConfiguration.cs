using HomeEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeEase.Infrastructure.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            // Property configurations
            builder.Property(b => b.ServicePrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.Currency)
                .IsRequired()
                .HasDefaultValue("USD");

            builder.Property(b => b.Notes)
                .HasMaxLength(500);

            builder.Property(b => b.CustomerAddress)
                .HasMaxLength(200);

            builder.Property(b => b.CancellationReason)
                .HasMaxLength(500);

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(b => b.AppointmentDate)
                .IsRequired();

            builder.Property(b => b.AppointmentTime)
                .IsRequired();

            builder.Property(b => b.IsHomeService)
                .IsRequired();

            builder.Property(b => b.DurationMinutes)
                .IsRequired();

            builder.Property(b => b.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Ignore computed property
            builder.Ignore(b => b.AppointmentDateTime);

            // Relationships
            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(b => b.Provider)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.ProviderId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();


            builder.HasOne(b => b.Review)
                .WithOne(r => r.Booking)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(b => b.Payments)
                .WithOne(p => p.Booking)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}