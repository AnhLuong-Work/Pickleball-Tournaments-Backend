using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPickleball.Infrastructure.Persistence.Configurations;

public class UserAuthProviderConfiguration : IEntityTypeConfiguration<UserAuthProvider>
{
    public void Configure(EntityTypeBuilder<UserAuthProvider> builder)
    {
        builder.ToTable("UserAuthProviders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Provider).IsRequired().HasMaxLength(20);
        builder.Property(x => x.ProviderUserId).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.Provider }).IsUnique();
    }
}
