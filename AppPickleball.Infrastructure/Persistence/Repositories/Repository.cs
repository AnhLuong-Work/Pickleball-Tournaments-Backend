using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

// Base repository — mọi entity đều dùng được
// HasQueryFilter trong Configurations tự động filter deleted_at IS NULL cho soft-delete entities
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppPickleballDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppPickleballDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ─── Query ───

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // FindAsync bypass GlobalQueryFilter → kiểm tra thủ công
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);

        if (entity is BaseEntity baseEntity && baseEntity.IsDeleted)
            return null;

        return entity;
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // GlobalQueryFilter tự filter deleted_at IS NULL
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            e => EF.Property<Guid>(e, "Id") == id,
            cancellationToken);
    }

    // ─── Command ───

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    // Hard delete — dùng cho token tables, log tables (BaseCreatedEntity)
    public virtual void Remove(T entity)
        => _dbSet.Remove(entity);
}


