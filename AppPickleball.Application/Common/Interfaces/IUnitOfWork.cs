namespace AppPickleball.Application.Common.Interfaces;

// Unit of Work — gom nhiều operations thành 1 transaction, commit 1 lần duy nhất
public interface IUnitOfWork : IDisposable
{
    // Commit tất cả thay đổi trong 1 transaction
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
