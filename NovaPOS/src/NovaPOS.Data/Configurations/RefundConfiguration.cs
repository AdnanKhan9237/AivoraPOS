using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.RefundAmount).HasPrecision(18, 2);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.OriginalSale)
            .WithMany(x => x.Refunds)
            .HasForeignKey(x => x.OriginalSaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProcessedBy)
            .WithMany(x => x.ProcessedRefunds)
            .HasForeignKey(x => x.ProcessedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
