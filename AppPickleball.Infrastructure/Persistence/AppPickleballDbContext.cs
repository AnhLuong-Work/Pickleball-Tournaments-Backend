using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AppPickleball.Infrastructure.Persistence;

public class AppPickleballDbContext : DbContext, IBaseDbContext
{
    public AppPickleballDbContext(DbContextOptions<AppPickleballDbContext> options) : base(options) { }

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
