using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class ReturnConfiguration : IEntityTypeConfiguration<Return>
{
    public void Configure(EntityTypeBuilder<Return> builder)
    {
        builder.ToTable("Returns");
        builder.HasIndex(x => x.ReturnNumber).IsUnique();
        builder.Property(x => x.ReturnNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.RefundMethod).HasConversion<int>();

        builder.HasOne(x => x.OriginalSale)
            .WithMany(x => x.Returns)
            .HasForeignKey(x => x.OriginalSaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProcessedBy)
            .WithMany(x => x.ProcessedReturns)
            .HasForeignKey(x => x.ProcessedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
