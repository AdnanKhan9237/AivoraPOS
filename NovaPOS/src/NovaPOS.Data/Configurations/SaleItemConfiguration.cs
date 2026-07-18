using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductName).IsRequired();
        builder.Property(x => x.ProductSku).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.TaxRate).HasPrecision(18, 4);
        builder.Property(x => x.Discount).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.HasOne(x => x.Sale)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.SaleItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
