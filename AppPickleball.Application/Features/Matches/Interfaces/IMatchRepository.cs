using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Features.Matches.Interfaces;

public interface IMatchRepository : IRepository<Match>, ISoftDeletableRepository<Match>
{
    Task<List<Match>> GetByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default);
    Task<List<Match>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken = default);
}
