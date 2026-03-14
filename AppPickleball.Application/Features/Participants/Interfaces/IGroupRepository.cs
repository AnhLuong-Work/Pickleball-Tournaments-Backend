using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Features.Participants.Interfaces;

public interface IGroupRepository : IRepository<Group>
{
    Task<List<Group>> GetByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default);
    Task<Group?> GetWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default);
}
