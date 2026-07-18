using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.Property(x => x.PaymentMethod).HasConversion<int>();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);

        builder.HasOne(x => x.Sale)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
