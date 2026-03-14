using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

// Soft delete repository — extends Repository<T> so concrete repos only need one base class
public class SoftDeletableRepository<T> : Repository<T>, ISoftDeletableRepository<T> where T : BaseEntity
{
    public SoftDeletableRepository(AppPickleballDbContext context) : base(context) { }

    public virtual void SoftDelete(T entity, Guid deletedBy)
    {
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;
        _dbSet.Update(entity);
    }

    public virtual void Restore(T entity)
    {
        entity.DeletedAt = null;
        entity.DeletedBy = null;
        _dbSet.Update(entity);
    }

    public virtual async Task<T?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<List<T>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);
    }
}
