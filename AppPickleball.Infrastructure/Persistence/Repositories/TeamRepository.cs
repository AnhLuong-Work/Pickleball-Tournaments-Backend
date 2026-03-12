using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class TeamRepository : Repository<Team>, ITeamRepository
{
    public TeamRepository(AppPickleballDbContext context) : base(context) { }

    public async Task<List<Team>> GetByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        => await _dbSet.Include(t => t.Player1).Include(t => t.Player2)
            .Where(t => t.TournamentId == tournamentId)
            .ToListAsync(cancellationToken);
}
