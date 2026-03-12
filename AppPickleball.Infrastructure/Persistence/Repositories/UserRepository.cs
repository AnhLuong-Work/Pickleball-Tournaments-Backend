using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, ISoftDeletableRepository<User>, IUserRepository
{
    private readonly SoftDeletableRepository<User> _softDeletable;

    public UserRepository(AppPickleballDbContext context) : base(context)
    {
        _softDeletable = new SoftDeletableRepository<User>(context);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);

    // ISoftDeletableRepository delegation
    public void SoftDelete(User entity, Guid deletedBy) => _softDeletable.SoftDelete(entity, deletedBy);
    public void Restore(User entity) => _softDeletable.Restore(entity);
    public Task<User?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
        => _softDeletable.GetByIdIncludingDeletedAsync(id, ct);
    public Task<List<User>> GetAllIncludingDeletedAsync(CancellationToken ct = default)
        => _softDeletable.GetAllIncludingDeletedAsync(ct);
}
