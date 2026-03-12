using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPickleball.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.PasswordHash).HasMaxLength(255);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
        builder.Property(x => x.Bio).HasColumnType("text");
        builder.Property(x => x.SkillLevel).HasPrecision(2, 1);
        builder.Property(x => x.DominantHand).HasMaxLength(10);
        builder.Property(x => x.PaddleType).HasMaxLength(100);
        builder.Property(x => x.EmailVerificationToken).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.DeletedAt).HasColumnType("timestamptz");
        builder.Property(x => x.EmailVerifiedAt).HasColumnType("timestamptz");
        builder.Property(x => x.EmailVerificationTokenExpiresAt).HasColumnType("timestamptz");
        builder.Property(x => x.PasswordResetToken).HasMaxLength(500);
        builder.Property(x => x.PasswordResetTokenExpiresAt).HasColumnType("timestamptz");

        builder.HasIndex(x => x.Email).IsUnique();

        // Soft delete filter
        builder.HasQueryFilter(x => x.DeletedAt == null);

        // Ignore computed property
        builder.Ignore(x => x.IsDeleted);

        // Relationships
        builder.HasMany(x => x.RefreshTokens).WithOne(x => x.User)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.AuthProviders).WithOne(x => x.User)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.CreatedTournaments).WithOne(x => x.Creator)
            .HasForeignKey(x => x.CreatorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Following).WithOne(x => x.Follower)
            .HasForeignKey(x => x.FollowerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Followers).WithOne(x => x.FollowingUser)
            .HasForeignKey(x => x.FollowingId).OnDelete(DeleteBehavior.Cascade);
    }
}
