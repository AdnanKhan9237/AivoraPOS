using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Barcode).HasMaxLength(100);
        builder.Property(x => x.PurchasePrice).HasPrecision(18, 2);
        builder.Property(x => x.SalePrice).HasPrecision(18, 2);
        builder.Property(x => x.TaxRate).HasPrecision(18, 4).HasDefaultValue(0m);
        builder.Property(x => x.LowStockThreshold).HasDefaultValue(5);

        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => x.Barcode);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
