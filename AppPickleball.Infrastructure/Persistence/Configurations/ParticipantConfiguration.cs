using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPickleball.Infrastructure.Persistence.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.RejectReason).HasMaxLength(500);
        builder.Property(x => x.JoinedAt).HasColumnType("timestamptz");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        builder.HasIndex(x => new { x.TournamentId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.TournamentId, x.Status });

        builder.HasOne(x => x.User).WithMany(x => x.Participants)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.InvitedByUser).WithMany()
            .HasForeignKey(x => x.InvitedBy).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Tournament).WithMany(x => x.Participants)
            .HasForeignKey(x => x.TournamentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.Tournament.DeletedAt == null && x.User.DeletedAt == null);
    }
}
