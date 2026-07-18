using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasIndex(x => x.SaleNumber).IsUnique();
        builder.Property(x => x.SaleNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.Cashier)
            .WithMany(x => x.Sales)
            .HasForeignKey(x => x.CashierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
