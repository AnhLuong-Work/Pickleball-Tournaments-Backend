using AppPickleball.Domain.Entities;
using AppPickleball.Domain.Enums;

namespace AppPickleball.Application.Common.Interfaces;

public interface ITournamentRepository : IRepository<Tournament>, ISoftDeletableRepository<Tournament>
{
    Task<Tournament?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
