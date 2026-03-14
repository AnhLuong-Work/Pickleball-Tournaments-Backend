using AppPickleball.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppPickleball.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppPickleballDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed = false;

    public UnitOfWork(AppPickleballDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Commit toàn bộ change tracker trong 1 implicit transaction.
    /// EF Core tự động wrap thành 1 DB transaction — atomic by default.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// Bắt đầu explicit transaction.
    /// Dùng khi handler cần nhiều lần SaveChanges mà vẫn muốn rollback toàn bộ nếu có lỗi.
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException("Đã có transaction đang mở. Không thể nest transaction.");

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>Commit explicit transaction đang mở.</summary>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("Không có transaction đang mở để commit.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>Rollback explicit transaction đang mở.</summary>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
