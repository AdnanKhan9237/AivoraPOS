using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");
        builder.HasIndex(x => x.ProductId).IsUnique();
        builder.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
        builder.Property(x => x.ReorderLevel).HasPrecision(18, 3);

        builder.HasOne(x => x.Product)
            .WithOne(x => x.Inventory)
            .HasForeignKey<Inventory>(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
