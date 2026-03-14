using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Features.Teams.Interfaces;

public interface ITeamRepository : IRepository<Team>
{
    Task<List<Team>> GetByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default);
}
