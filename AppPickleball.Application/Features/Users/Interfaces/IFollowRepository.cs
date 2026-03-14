using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Features.Users.Interfaces;

public interface IFollowRepository : IRepository<Follow>
{
    Task<Follow?> GetByPairAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
}
