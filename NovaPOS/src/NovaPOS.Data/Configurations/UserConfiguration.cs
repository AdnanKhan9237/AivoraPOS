using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaPOS.Core.Entities;

namespace NovaPOS.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PinHash).IsRequired();
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.Role).HasConversion<int>();

        builder.HasIndex(x => x.Username).IsUnique();
    }
}
