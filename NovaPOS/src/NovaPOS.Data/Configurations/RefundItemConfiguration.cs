using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class RefundItemConfiguration : IEntityTypeConfiguration<RefundItem>
{
    public void Configure(EntityTypeBuilder<RefundItem> builder)
    {
        builder.ToTable("RefundItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RefundAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Refund)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.RefundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SaleItem)
            .WithMany(x => x.RefundItems)
            .HasForeignKey(x => x.SaleItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
