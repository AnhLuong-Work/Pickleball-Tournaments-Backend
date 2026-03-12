using AppPickleball.Domain.Common;

namespace AppPickleball.Application.Common.Interfaces;

// Soft delete repository — CHỈ cho entities kế thừa BaseEntity (có DeletedAt, DeletedBy)
// Constraint where T : BaseEntity đảm bảo compile-time safety
public interface ISoftDeletableRepository<T> where T : BaseEntity
{
    void SoftDelete(T entity, Guid deletedBy);
    void Restore(T entity);
    Task<T?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
}
