using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Tournaments.Interfaces;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class TournamentRepository : SoftDeletableRepository<Tournament>, ITournamentRepository
{
    public TournamentRepository(AppPickleballDbContext context) : base(context) { }

    public async Task<Tournament?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Creator)
            .Include(t => t.Groups).ThenInclude(g => g.Members).ThenInclude(m => m.Player)
            .Include(t => t.Groups).ThenInclude(g => g.Members).ThenInclude(m => m.Team)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
}
