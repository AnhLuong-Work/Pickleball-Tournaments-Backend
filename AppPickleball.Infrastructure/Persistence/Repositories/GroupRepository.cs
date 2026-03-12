using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class GroupRepository : Repository<Group>, IGroupRepository
{
    public GroupRepository(AppPickleballDbContext context) : base(context) { }

    public async Task<List<Group>> GetByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        => await _dbSet.Include(g => g.Members).ThenInclude(m => m.Player)
            .Include(g => g.Members).ThenInclude(m => m.Team).ThenInclude(t => t!.Player1)
            .Include(g => g.Members).ThenInclude(m => m.Team).ThenInclude(t => t!.Player2)
            .Where(g => g.TournamentId == tournamentId)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<Group?> GetWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default)
        => await _dbSet.Include(g => g.Members).ThenInclude(m => m.Player)
            .Include(g => g.Members).ThenInclude(m => m.Team)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
}
