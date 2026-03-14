using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Matches.Interfaces;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class MatchRepository : SoftDeletableRepository<Match>, IMatchRepository
{
    public MatchRepository(AppPickleballDbContext context) : base(context) { }

    public async Task<List<Match>> GetByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        => await _dbSet.Include(m => m.Group)
            .Where(m => m.TournamentId == tournamentId)
            .OrderBy(m => m.Group.DisplayOrder).ThenBy(m => m.Round).ThenBy(m => m.MatchOrder)
            .ToListAsync(cancellationToken);

    public async Task<List<Match>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
        => await _dbSet.Where(m => m.GroupId == groupId)
            .OrderBy(m => m.Round).ThenBy(m => m.MatchOrder)
            .ToListAsync(cancellationToken);
}
