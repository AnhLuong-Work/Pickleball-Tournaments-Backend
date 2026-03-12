using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class TournamentRepository : Repository<Tournament>, ISoftDeletableRepository<Tournament>, ITournamentRepository
{
    private readonly SoftDeletableRepository<Tournament> _softDeletable;

    public TournamentRepository(AppPickleballDbContext context) : base(context)
    {
        _softDeletable = new SoftDeletableRepository<Tournament>(context);
    }

    public async Task<Tournament?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Creator)
            .Include(t => t.Groups).ThenInclude(g => g.Members).ThenInclude(m => m.Player)
            .Include(t => t.Groups).ThenInclude(g => g.Members).ThenInclude(m => m.Team)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public void SoftDelete(Tournament entity, Guid deletedBy) => _softDeletable.SoftDelete(entity, deletedBy);
    public void Restore(Tournament entity) => _softDeletable.Restore(entity);
    public Task<Tournament?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
        => _softDeletable.GetByIdIncludingDeletedAsync(id, ct);
    public Task<List<Tournament>> GetAllIncludingDeletedAsync(CancellationToken ct = default)
        => _softDeletable.GetAllIncludingDeletedAsync(ct);
}
