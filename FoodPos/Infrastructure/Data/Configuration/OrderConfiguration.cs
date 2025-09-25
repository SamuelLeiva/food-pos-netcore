using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order");
        builder.Property(o => o.Id)
            .IsRequired();
        builder.Property(o => o.Status)
            .IsRequired();
        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");
        builder.Property(o => o.PaymentIntentId)
            .IsRequired();
        builder.Property(o => o.StripeCustomerId)
            .IsRequired();
        builder.Property(o => o.ReceiptEmail)
            .HasMaxLength(200);
    }
}
