using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPickleball.Infrastructure.Persistence.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(10);

        builder.HasIndex(x => new { x.TournamentId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.TournamentId, x.DisplayOrder }).IsUnique();
        builder.HasIndex(x => x.TournamentId);

        builder.HasOne(x => x.Tournament).WithMany(x => x.Groups)
            .HasForeignKey(x => x.TournamentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.Tournament.DeletedAt == null);
    }
}
