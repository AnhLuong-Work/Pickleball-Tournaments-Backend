using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPickleball.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(15);
        builder.Property(x => x.Player1Scores).HasColumnType("jsonb");
        builder.Property(x => x.Player2Scores).HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.DeletedAt).HasColumnType("timestamptz");

        builder.HasIndex(x => x.TournamentId);
        builder.HasIndex(x => x.GroupId);
        builder.HasIndex(x => new { x.GroupId, x.Status });
        builder.HasIndex(x => new { x.GroupId, x.Round, x.MatchOrder }).IsUnique();

        builder.HasQueryFilter(x => x.DeletedAt == null);
        builder.Ignore(x => x.IsDeleted);

        builder.HasOne(x => x.Tournament).WithMany(x => x.Matches)
            .HasForeignKey(x => x.TournamentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Group).WithMany(x => x.Matches)
            .HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
    }
}
