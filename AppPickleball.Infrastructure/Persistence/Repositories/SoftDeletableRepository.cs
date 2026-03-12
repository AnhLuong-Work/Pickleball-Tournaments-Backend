using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

// Soft delete repository — CHỈ cho entities kế thừa BaseEntity
// Constraint where T : BaseEntity = compile-time safety (không thể gọi SoftDelete trên BaseCreatedEntity)
public class SoftDeletableRepository<T> : ISoftDeletableRepository<T> where T : BaseEntity
{
    private readonly AppPickleballDbContext _context;
    private readonly DbSet<T> _dbSet;

    public SoftDeletableRepository(AppPickleballDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

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
