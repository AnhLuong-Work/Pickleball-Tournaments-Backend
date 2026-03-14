using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Users.Interfaces;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class FollowRepository : Repository<Follow>, IFollowRepository
{
    public FollowRepository(AppPickleballDbContext context) : base(context) { }

    public async Task<Follow?> GetByPairAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, cancellationToken);

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, cancellationToken);
}
