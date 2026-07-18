using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AivoraPOS.Core.Entities;

namespace AivoraPOS.Data.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MovementType).HasConversion<int>();
        builder.Property(x => x.Reference).HasMaxLength(200);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.InventoryMovements)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.InventoryMovements)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
