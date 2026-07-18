using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasIndex(x => x.Username).IsUnique();
        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PinHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Role).HasConversion<int>();
    }
}
