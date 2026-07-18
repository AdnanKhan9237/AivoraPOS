using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
