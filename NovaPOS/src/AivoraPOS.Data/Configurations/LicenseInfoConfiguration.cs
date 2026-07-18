using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AivoraPOS.Core.Entities;

namespace AivoraPOS.Data.Configurations;

public class LicenseInfoConfiguration : IEntityTypeConfiguration<LicenseInfo>
{
    public void Configure(EntityTypeBuilder<LicenseInfo> builder)
    {
        builder.ToTable("LicenseInfo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LicenseKey).IsRequired();
        builder.Property(x => x.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.HardwareFingerprint).IsRequired();
        builder.Property(x => x.Plan).HasConversion<int>();
    }
}
