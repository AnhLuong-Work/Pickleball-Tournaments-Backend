using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;

namespace AppPickleball.Application.Common.Interfaces;

public interface IParticipantRepository : IRepository<Participant>
{
    Task<Participant?> GetByTournamentAndUserAsync(Guid tournamentId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<Participant>> GetByTournamentAsync(Guid tournamentId, ParticipantStatus? status = null, CancellationToken cancellationToken = default);
    Task<int> CountConfirmedAsync(Guid tournamentId, CancellationToken cancellationToken = default);
}
