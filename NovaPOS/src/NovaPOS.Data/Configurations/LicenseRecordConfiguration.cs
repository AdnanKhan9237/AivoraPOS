using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class LicenseRecordConfiguration : IEntityTypeConfiguration<LicenseRecord>
{
    public void Configure(EntityTypeBuilder<LicenseRecord> builder)
    {
        builder.ToTable("LicenseRecords");
        builder.Property(x => x.LicenseKeyHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.HardwareFingerprint).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CompanyName).HasMaxLength(200);
    }
}
