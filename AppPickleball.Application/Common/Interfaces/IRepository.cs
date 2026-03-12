using AppPickleball.Domain.Common;

namespace AppPickleball.Application.Common.Interfaces;

// Base repository — mọi entity đều dùng được (không phụ thuộc BaseEntity hay BaseCreatedEntity)
public interface IRepository<T> where T : class
{
    // Query
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    // Command — không gọi SaveChanges, để UnitOfWork commit
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);                                 // Hard delete
}


