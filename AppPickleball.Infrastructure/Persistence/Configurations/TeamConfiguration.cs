using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPickleball.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        builder.HasIndex(x => x.TournamentId);

        builder.HasOne(x => x.Tournament).WithMany(x => x.Teams)
            .HasForeignKey(x => x.TournamentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Player1).WithMany()
            .HasForeignKey(x => x.Player1Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Player2).WithMany()
            .HasForeignKey(x => x.Player2Id).OnDelete(DeleteBehavior.Restrict);
    }
}
