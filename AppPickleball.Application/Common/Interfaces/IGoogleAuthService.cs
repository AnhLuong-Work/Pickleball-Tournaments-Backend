namespace AppPickleball.Application.Common.Interfaces;

public record GoogleUserInfo(string GoogleId, string Email, string? Name, string? Picture);

public interface IGoogleAuthService
{
    Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken, CancellationToken ct = default);
}
