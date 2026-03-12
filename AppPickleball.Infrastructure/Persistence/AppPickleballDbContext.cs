using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Common;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AppPickleball.Infrastructure.Persistence;

public class AppPickleballDbContext : DbContext, IBaseDbContext
{
    public AppPickleballDbContext(DbContextOptions<AppPickleballDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserAuthProvider> UserAuthProviders => Set<UserAuthProvider>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchScoreHistory> MatchScoreHistories => Set<MatchScoreHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Tự động set UpdatedAt cho tất cả entity đang Modified
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
