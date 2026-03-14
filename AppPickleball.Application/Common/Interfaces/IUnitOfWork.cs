namespace AppPickleball.Application.Common.Interfaces;

/// <summary>
/// Unit of Work — gom nhiều operations thành 1 transaction, commit 1 lần duy nhất.
///
/// Dùng SaveChangesAsync khi handler chỉ cần 1 lần commit (phổ biến nhất).
/// Dùng BeginTransactionAsync khi cần kiểm soát thủ công — ví dụ:
///   - Có nhiều lần SaveChanges trong cùng 1 handler
///   - Cần rollback có điều kiện sau khi gọi external service
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Commit tất cả pending changes vào DB trong 1 implicit transaction.
    /// EF Core tự wrap toàn bộ thay đổi trong change tracker vào 1 DB transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bắt đầu explicit transaction. Dùng khi cần nhiều lần SaveChanges
    /// trong cùng 1 handler mà vẫn muốn atomicity.
    /// Phải gọi CommitAsync hoặc RollbackAsync sau đó.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Commit explicit transaction đang mở.</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>Rollback explicit transaction đang mở.</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
