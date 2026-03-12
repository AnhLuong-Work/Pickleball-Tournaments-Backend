using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Common.Interfaces;

public interface ITeamRepository : IRepository<Team>
{
    Task<List<Team>> GetByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default);
}
