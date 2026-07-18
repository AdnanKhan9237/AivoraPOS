using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => x.Barcode);
        builder.Property(x => x.Sku).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Barcode).HasMaxLength(50);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.CostPrice).HasPrecision(18, 2);
        builder.Property(x => x.TaxRate).HasPrecision(5, 4);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
