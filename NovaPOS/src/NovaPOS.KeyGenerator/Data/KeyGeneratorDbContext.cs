using Microsoft.EntityFrameworkCore;
using NovaPOS.KeyGenerator.Entities;

namespace NovaPOS.KeyGenerator.Data;

public class KeyGeneratorDbContext : DbContext
{
  public KeyGeneratorDbContext(DbContextOptions<KeyGeneratorDbContext> options) : base(options)
  {
  }

  public DbSet<GeneratedLicenseKey> GeneratedLicenseKeys => Set<GeneratedLicenseKey>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<GeneratedLicenseKey>(entity =>
    {
      entity.ToTable("GeneratedLicenseKeys");
      entity.HasKey(x => x.Id);
      entity.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
      entity.Property(x => x.LicenseKey).HasMaxLength(30).IsRequired();
      entity.Property(x => x.Plan).HasConversion<int>();
      entity.HasIndex(x => x.GeneratedAtUtc);
      entity.HasIndex(x => x.LicenseKey).IsUnique();
    });
  }
}
