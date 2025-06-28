using HomeEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeEase.Infrastructure.Data.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> entity)
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.HomePrice).HasPrecision(10, 2);

            entity.HasOne(s => s.Provider)
                  .WithMany(p => p.Services)
                  .HasForeignKey(s => s.ProviderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.BasePlatformService)
                  .WithMany() 
                  .HasForeignKey(s => s.BasePlatformServiceId)
                  .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
