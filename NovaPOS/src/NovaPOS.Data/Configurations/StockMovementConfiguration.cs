using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.Property(x => x.MovementType).HasConversion<int>();
        builder.Property(x => x.QuantityChange).HasPrecision(18, 3);
        builder.Property(x => x.QuantityAfter).HasPrecision(18, 3);
        builder.Property(x => x.ReferenceType).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
