using HomeEase.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Infrastructure.Data.Configurations
{
    public class PaymentInfoConfiguration : IEntityTypeConfiguration<PaymentInfo>
    {
        public void Configure(EntityTypeBuilder<PaymentInfo> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .IsRequired();

            builder.Property(p => p.PaymentMethod)
                .IsRequired();

        }
    }
}
