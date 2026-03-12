using AppPickleball.Application.Common.Interfaces;

namespace AppPickleball.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppPickleballDbContext _context;
    private bool _disposed = false;

    public UnitOfWork(AppPickleballDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
