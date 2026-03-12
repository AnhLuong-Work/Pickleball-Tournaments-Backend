using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPickleball.Infrastructure.Persistence.Configurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("Tournaments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.NumGroups).IsRequired();
        builder.Property(x => x.ScoringFormat).HasConversion<string>().HasMaxLength(15);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(15);
        builder.Property(x => x.Location).HasMaxLength(500);
        builder.Property(x => x.BannerUrl).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.DeletedAt).HasColumnType("timestamptz");

        builder.HasIndex(x => x.CreatorId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Date);

        builder.HasQueryFilter(x => x.DeletedAt == null);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.MaxParticipants);
    }
}
