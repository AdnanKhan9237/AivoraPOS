using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AivoraPOS.Core.Entities;

namespace AivoraPOS.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SaleNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.AmountPaid).HasPrecision(18, 2);
        builder.Property(x => x.Change).HasPrecision(18, 2);
        builder.Property(x => x.PaymentMethod).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasIndex(x => x.SaleNumber).IsUnique();
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Cashier)
            .WithMany(x => x.Sales)
            .HasForeignKey(x => x.CashierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
