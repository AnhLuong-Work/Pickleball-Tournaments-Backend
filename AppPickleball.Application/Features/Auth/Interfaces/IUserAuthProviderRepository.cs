using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Features.Auth.Interfaces;

public interface IUserAuthProviderRepository : IRepository<UserAuthProvider>
{
    Task<UserAuthProvider?> FindByProviderAsync(string provider, string providerUserId, CancellationToken ct = default);
}
