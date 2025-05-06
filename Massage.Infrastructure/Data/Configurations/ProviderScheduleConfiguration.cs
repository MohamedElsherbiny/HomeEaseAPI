using Massage.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Infrastructure.Data.Configurations
{
    public class ProviderScheduleConfiguration : IEntityTypeConfiguration<ProviderSchedule>
    {
        public void Configure(EntityTypeBuilder<ProviderSchedule> entity)
        {
            // Key and basic properties
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            // Relationship with Provider (one-to-one)
            entity.HasOne(s => s.Provider)
                .WithOne(p => p.Schedule)
                .HasForeignKey<ProviderSchedule>(s => s.ProviderId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // Relationships with child collections
            entity.HasMany(s => s.RegularHours)
                .WithOne(w => w.ProviderSchedule)
                .HasForeignKey(w => w.ProviderScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.SpecialDates)
                .WithOne(sd => sd.ProviderSchedule)
                .HasForeignKey(sd => sd.ProviderScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.AvailableSlots)
                .WithOne(ts => ts.ProviderSchedule)
                .HasForeignKey(ts => ts.ProviderScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
    {
        public void Configure(EntityTypeBuilder<WorkingHours> entity)
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            // Create a unique constraint for provider schedule + day of week
            entity.HasIndex(e => new { e.ProviderScheduleId, e.DayOfWeek }).IsUnique();

            entity.Property(e => e.DayOfWeek).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
            entity.Property(e => e.IsOpen).IsRequired();
        }
    }

    public class SpecialDateConfiguration : IEntityTypeConfiguration<SpecialDate>
    {
        public void Configure(EntityTypeBuilder<SpecialDate> entity)
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            // Create a unique constraint for provider schedule + date
            entity.HasIndex(e => new { e.ProviderScheduleId, e.Date }).IsUnique();

            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.IsClosed).IsRequired();
            entity.Property(e => e.Note).HasMaxLength(200);
        }
    }

    public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
    {
        public void Configure(EntityTypeBuilder<TimeSlot> entity)
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            // Create a unique constraint for provider schedule + start time + end time
            entity.HasIndex(e => new { e.ProviderScheduleId, e.StartTime, e.EndTime }).IsUnique();

            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
            entity.Property(e => e.IsAvailable).IsRequired();
        }
    }
}
