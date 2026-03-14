using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Features.Auth.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
