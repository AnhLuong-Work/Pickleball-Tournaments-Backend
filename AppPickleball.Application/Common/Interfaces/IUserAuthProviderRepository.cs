using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Common.Interfaces;

public interface IUserAuthProviderRepository : IRepository<UserAuthProvider>
{
    Task<UserAuthProvider?> FindByProviderAsync(string provider, string providerUserId, CancellationToken ct = default);
}
