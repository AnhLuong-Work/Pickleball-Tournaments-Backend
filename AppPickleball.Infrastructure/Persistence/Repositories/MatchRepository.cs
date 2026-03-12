using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class MatchRepository : Repository<Match>, ISoftDeletableRepository<Match>, IMatchRepository
{
    private readonly SoftDeletableRepository<Match> _softDeletable;

    public MatchRepository(AppPickleballDbContext context) : base(context)
    {
        _softDeletable = new SoftDeletableRepository<Match>(context);
    }

    public async Task<List<Match>> GetByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        => await _dbSet.Include(m => m.Group)
            .Where(m => m.TournamentId == tournamentId)
            .OrderBy(m => m.Group.DisplayOrder).ThenBy(m => m.Round).ThenBy(m => m.MatchOrder)
            .ToListAsync(cancellationToken);

    public async Task<List<Match>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
        => await _dbSet.Where(m => m.GroupId == groupId)
            .OrderBy(m => m.Round).ThenBy(m => m.MatchOrder)
            .ToListAsync(cancellationToken);

    public void SoftDelete(Match entity, Guid deletedBy) => _softDeletable.SoftDelete(entity, deletedBy);
    public void Restore(Match entity) => _softDeletable.Restore(entity);
    public Task<Match?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
        => _softDeletable.GetByIdIncludingDeletedAsync(id, ct);
    public Task<List<Match>> GetAllIncludingDeletedAsync(CancellationToken ct = default)
        => _softDeletable.GetAllIncludingDeletedAsync(ct);
}
