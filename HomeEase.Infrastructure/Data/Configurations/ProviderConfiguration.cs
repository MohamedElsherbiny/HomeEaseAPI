using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HomeEase.Domain.Entities;

namespace HomeEase.Infrastructure.Data.Configurations
{
    public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
    {
        public void Configure(EntityTypeBuilder<Provider> entity)
        {
            // Key and basic properties
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BusinessName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Rating).HasPrecision(3, 2);

            // Conversion for ServiceTypes
            entity.Property(e => e.ServiceTypes).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            // Relationships
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Address)
                  .WithMany(a => a.Providers)
                  .HasForeignKey(p => p.AddressId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);

            entity.HasMany(p => p.Services)
                .WithOne(s => s.Provider)
                .HasForeignKey(s => s.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Locations)
                .WithOne(l => l.Provider)
                .HasForeignKey(l => l.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Bookings)
                .WithOne(b => b.Provider)
                .HasForeignKey(b => b.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Schedule)
                .WithOne(s => s.Provider)
                .HasForeignKey<ProviderSchedule>(s => s.ProviderId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            entity.HasMany(p => p.Images)
                .WithOne(pi => pi.Provider)
                .HasForeignKey(pi => pi.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
