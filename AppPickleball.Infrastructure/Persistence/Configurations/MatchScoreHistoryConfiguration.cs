using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPickleball.Infrastructure.Persistence.Configurations;

public class MatchScoreHistoryConfiguration : IEntityTypeConfiguration<MatchScoreHistory>
{
    public void Configure(EntityTypeBuilder<MatchScoreHistory> builder)
    {
        builder.ToTable("MatchScoreHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.OldPlayer1Scores).HasColumnType("jsonb");
        builder.Property(x => x.OldPlayer2Scores).HasColumnType("jsonb");
        builder.Property(x => x.NewPlayer1Scores).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.NewPlayer2Scores).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        builder.HasIndex(x => x.MatchId);

        builder.HasOne(x => x.Match).WithMany(x => x.ScoreHistories)
            .HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ModifiedByUser).WithMany()
            .HasForeignKey(x => x.ModifiedBy).OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.Match.DeletedAt == null);
    }
}
