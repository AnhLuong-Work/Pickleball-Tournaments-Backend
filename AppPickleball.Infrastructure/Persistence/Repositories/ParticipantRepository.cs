using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Participants.Interfaces;
using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class ParticipantRepository : Repository<Participant>, IParticipantRepository
{
    public ParticipantRepository(AppPickleballDbContext context) : base(context) { }

    public async Task<Participant?> GetByTournamentAndUserAsync(Guid tournamentId, Guid userId, CancellationToken cancellationToken = default)
        => await _dbSet.Include(p => p.User)
            .FirstOrDefaultAsync(p => p.TournamentId == tournamentId && p.UserId == userId, cancellationToken);

    public async Task<List<Participant>> GetByTournamentAsync(Guid tournamentId, ParticipantStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(p => p.User).Where(p => p.TournamentId == tournamentId);
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);
        return await query.OrderBy(p => p.JoinedAt ?? p.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<int> CountConfirmedAsync(Guid tournamentId, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(p => p.TournamentId == tournamentId && p.Status == ParticipantStatus.Confirmed, cancellationToken);
}
