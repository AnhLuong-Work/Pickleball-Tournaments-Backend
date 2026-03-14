using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Features.Auth.Interfaces;
using AppPickleball.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPickleball.Infrastructure.Persistence.Repositories;

public class UserAuthProviderRepository : Repository<UserAuthProvider>, IUserAuthProviderRepository
{
    public UserAuthProviderRepository(AppPickleballDbContext context) : base(context) { }

    public async Task<UserAuthProvider?> FindByProviderAsync(string provider, string providerUserId, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(
            x => x.Provider == provider && x.ProviderUserId == providerUserId, ct);
}
